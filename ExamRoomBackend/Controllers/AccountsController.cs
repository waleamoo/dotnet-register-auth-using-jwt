using ExamRoomBackend.Models;
using ExamRoomBackend.Models.DTO;
using ExamRoomBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Twilio.Rest.Chat.V1;
using Twilio.TwiML.Voice;

namespace ExamRoomBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _config;

        public AccountsController(IUserRepository userRepository, IEmailService emailService, IConfiguration config)
        {
            _userRepository = userRepository;
            _emailService = emailService;
            _config = config;
        }


        [HttpPost("login")]
        public async Task<IActionResult> Authenticate(LoginReqDto loginReq)
        {
            var user = await _userRepository.Authenticate(loginReq.Email, loginReq.Password);
            if (user == null)
                return Unauthorized("User not found");
            // if login is successful 
            var loginResponse = new LoginResDto();
            loginResponse.Email = user.Email;
            loginResponse.FirstName = user.FirstName;
            loginResponse.LastName = user.LastName;
            loginResponse.Token = _userRepository.CreateJWT(user);
            return Ok(loginResponse);
        }

        [HttpGet("GetMe"), Authorize]
        public ActionResult<string> GetMe()
        {
            var userName = _userRepository.GetMyName();
            return Ok(userName);
            //var userName = User?.Identity?.Name;
            //var userName2 = User.FindFirstValue(ClaimTypes.Name);
            //var role = User.FindFirstValue(ClaimTypes.Role);
            //return Ok(new { userName, userName2, role });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterReqDto registerReqDto)
        {
            try
            {
                var userExists = await _userRepository.UserAlreadyExists(registerReqDto.Email);
                // verify if the user exists 
                if (userExists)
                    return BadRequest("User already exist, please try something else");
                // if the user does not exists - create the new use
                _userRepository.Register(registerReqDto.Email, registerReqDto.Password, registerReqDto.FirstName, registerReqDto.LastName);

                return StatusCode(201, "User created successfully - Please confirm your email address.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            
        }


        [HttpGet("verifyUser")]
        public async Task<IActionResult> VerifyUser(string token)
        {
            try
            {
                var isVerfied = await _userRepository.VerifyUser(token);
                if (isVerfied)
                    return Ok("User verified successfully");
                // if the user does not exists
                return BadRequest("User not found.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }

        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            try
            {
                if (await _userRepository.ForgotPassword(email))
                {
                    return Ok($"If an account exists for {email}. A password reset link has been sent.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            return BadRequest($"If an account exists for {email}. Account not found");
        }
        
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel request)
        {
            // the front-end get the PasswordResetToken from the url - and the backend performs the password reset using the ResetPasswordModel
            try
            {
                if (await _userRepository.ResetPassword(request))
                {
                    return Ok($"Password reset success.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
            return BadRequest($"Error occured: Invalid token and/or expired token.");
        }

        [HttpGet("email-inactive-users")]
        public async Task<IActionResult> EmailInActiveUsers()
        {
            try
            {
                if (await _userRepository.EmailInActiveUsers())
                {
                    return Ok($"All inactive users emailed successfully.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"No inactive users found");
            }
            return BadRequest($"No inactive users found");
        }

        [HttpDelete("deleteCandidiate")]
        public async Task<IActionResult> DeleteCandidiate(int id)
        {
            try
            {
                var deleteSuccess = await _userRepository.DeleteCandidiate(id);
                // verify if the user exists 
                if (deleteSuccess)
                    return Ok("Candiate deleted");
                return BadRequest("Candidiate not found.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }

        }

        
    }
}
