using AutoMapper;
using ExamRoomBackend.Helpers;
using ExamRoomBackend.Models;
using ExamRoomBackend.Models.Data;
using ExamRoomBackend.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Twilio.TwiML.Voice;

namespace ExamRoomBackend.Services
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRepository(DataContext context, IMapper mapper, IConfiguration config, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _config = config;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<User> Authenticate(string email, string passwordText)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.IsActive == true); // check email and IsActive 
            if (user == null || user.PasswordKey == null)
                return null;
            // verify the user password 
            if (!MatchPasswordHash(passwordText, user.PasswordHash, user.PasswordKey)) // login req password 
                return null;
            return user;
        }

        private bool MatchPasswordHash(string passwordText, byte[] passwordHash, byte[] passwordKey)
        {
            using (var hmac = new HMACSHA512(passwordKey))
            {
                var hashPassword = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(passwordText));
                // compare the byte array 
                for (int i = 0; i < hashPassword.Length; i++)
                {
                    if (hashPassword[i] != passwordHash[i])
                        return false;
                }
                return true;
            }
        }

        public void Register(string email, string password, string firstName, string lastName)
        {
            byte[] passwordHash, passwordKey;
            using (var hmac = new HMACSHA512())
            {
                passwordKey = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
            // create new user
            User user = new User();
            user.Email = email;
            user.PasswordHash = passwordHash;
            user.PasswordKey = passwordKey;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.VerficationToken = CreateRandomToken();
            user.IsActive = false;
            // other fields 
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;

            // save the user  
            _context.Users.Add(user);
            _context.SaveChanges();

            // send confirmation email 
            string? appDomain = _config.GetSection("Application:AppDomain").Value;
            string? confirmationLink = _config.GetSection("Application:EmailConfirmation").Value;

            UserEmailOptions options = new UserEmailOptions
            {
                ToEmails = new List<string> { user.Email },
                PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                    new KeyValuePair<string, string>("{{ UserName }}", user.FirstName),
                    new KeyValuePair<string, string>("{{ Link }}", string.Format(appDomain + confirmationLink, user.VerficationToken))
                    }
            };
            _emailService.SendRegistrationConfirmationEmail(options);
        }

        // verify the user
        public async Task<bool> VerifyUser(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.VerficationToken == token);
            if (user == null)
            {
                return false;
            }
            else
            {
                user.VerficationToken = null;
                user.VerifiedAt = DateTime.Now;
                user.IsActive = true;
                await _context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteCandidiate(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            else
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
        }


        // forgot password 
        public async Task<bool> ForgotPassword(string email)
        {
            var user = await GetUser(email);
            if (user == null)
            {
                return false;
            }
            else
            {
                user.PasswordResetToken = CreateRandomToken();
                user.ResetTokenExpires = DateTime.Now.AddDays(1);
                user.IsActive = false;
                await _context.SaveChangesAsync();
                // Email the user the password reset link
                string? appDomain = _config.GetSection("Application:AppDomain").Value;
                string? forgotPassword = _config.GetSection("Application:ForgotPassword").Value;

                UserEmailOptions options = new UserEmailOptions
                {
                    ToEmails = new List<string> { user.Email },
                    PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                    new KeyValuePair<string, string>("{{ UserName }}", user.FirstName),
                    new KeyValuePair<string, string>("{{ Link }}", string.Format(appDomain + forgotPassword, user.Id, user.VerficationToken))
                    }
                };
                await _emailService.SendForgotPasswordLinkEmail(options);

                return true;
            }
        }

        public async Task<bool> ResetPassword(ResetPasswordModel request)
        {
            // get the user by the password reset token 
            var user = await _context.Users.FirstOrDefaultAsync(x => x.PasswordResetToken == request.Token);
            if (user == null || user.ResetTokenExpires < DateTime.Now)
            {
                return false;
            }

            // hash the password
            byte[] passwordHash, passwordKey;
            using (var hmac = new HMACSHA512())
            {
                passwordKey = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(request.Password));
            }

            user.PasswordHash = passwordHash;
            user.PasswordKey = passwordKey;
            user.PasswordResetToken = null;
            user.IsActive = true;
            await _context.SaveChangesAsync();
            // Email the user password has been reset successfully
            UserEmailOptions options = new UserEmailOptions
            {
                ToEmails = new List<string> { user.Email },
                PlaceHolders = new List<KeyValuePair<string, string>>()
                    {
                    new KeyValuePair<string, string>("{{ UserName }}", user.FirstName)
                    }
            };
            await _emailService.SendPasswordChangedSuccessEmail(options);
            return true;
        }

        public async Task<bool> EmailInActiveUsers()
        {
            var users = await _context.Users.Where(x => x.IsActive == false).ToListAsync();
            if (users == null)
            {
                return false;
            }
            else
            {
                foreach (var user in users)
                {
                    UserEmailOptions options = new UserEmailOptions
                    {
                        ToEmails = new List<string> { user.Email },
                        PlaceHolders = new List<KeyValuePair<string, string>>()
                        {
                            new KeyValuePair<string, string>("{{ UserName }}", user.FirstName),
                        }
                    };
                    await _emailService.SendInActiveUsersEmail(options);
                }

                return true;
            }
        }

        // get the user by their email address 
        public async Task<User> GetUser(string email)
        {
            return await _context.Users.FirstAsync(x => x.Email == email);
        }

        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<bool> UserAlreadyExists(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email == email); // returns false
        }


        // create JWT 
        public string CreateJWT(User user)
        {
            // authentication key 
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));
            // create claim 
            var claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                //new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };
            // create signing credentials 
            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public string GetMyName()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }
    }
}
