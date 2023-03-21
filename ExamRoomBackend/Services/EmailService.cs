using ExamRoomBackend.Models;
using Microsoft.Extensions.Options;
using System.Text;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace ExamRoomBackend.Services
{
    public class EmailService : IEmailService
    {
        private const string templatePath = @"EmailTemplate/{0}.html";
        private readonly SMTPConfig _smtpConfig;
        private readonly ITwilioRestClient _client;


        public EmailService(IOptions<SMTPConfig> smtpConfig, ITwilioRestClient client)
        {
            _smtpConfig = smtpConfig.Value;
            _client = client;

        }

        // configure: helper method to send email
        private async Task SendEmail(UserEmailOptions userEmailOptions)
        {
            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage
            {
                Subject = userEmailOptions.Subject,
                Body = userEmailOptions.Body,
                From = new System.Net.Mail.MailAddress(_smtpConfig.SenderAddress, _smtpConfig.SenderDisplayName),
                IsBodyHtml = _smtpConfig.IsBodyHTML
            };

            foreach (var toEmail in userEmailOptions.ToEmails)
            {
                mail.To.Add(toEmail);
            }

            System.Net.Mail.SmtpClient smtpClient = new System.Net.Mail.SmtpClient
            {
                Host = _smtpConfig.Host,
                Port = _smtpConfig.Port,
                EnableSsl = _smtpConfig.EnableSSL,
                Credentials = new System.Net.NetworkCredential(_smtpConfig.UserName, _smtpConfig.Password)
            };


            mail.BodyEncoding = Encoding.Default;

            await smtpClient.SendMailAsync(mail);

            smtpClient.Dispose();
        }

        // configure: get email body
        private string GetEmailBody(string templateName)
        {
            //string templatePath = Path.Combine(_webHostingEnvironment.WebRootPath, @"EmailTemplate/{0}.html");
            //var body = File.ReadAllText(string.Format(templatePath, templateName));
            var body = File.ReadAllText(String.Format(templatePath, templateName));
            return body;
        }

        // configure: placeholder email content
        private string UpdatePlaceHolders(string text, List<KeyValuePair<string, string>> keyValuePairs)
        {
            if (!string.IsNullOrEmpty(text) && keyValuePairs != null)
            {
                foreach (var placeholder in keyValuePairs)
                {
                    if (text.Contains(placeholder.Key))
                    {
                        text = text.Replace(placeholder.Key, placeholder.Value);
                    }
                }
            }
            return text;
        }

        // 1. send verification email method - we can duplicate this type of method to send more emails  
        public async Task SendVerificationEmail(UserEmailOptions userEmailOptions)
        {
            // parameters
            var emails = new List<string>() { "test@test.com" }; // use the logged in user email

            // create new instance of email option 
            userEmailOptions.Subject = UpdatePlaceHolders("ExamRoom AI - Verification Code", userEmailOptions.PlaceHolders);
            userEmailOptions.Body = UpdatePlaceHolders(GetEmailBody("verify"), userEmailOptions.PlaceHolders);
            userEmailOptions.ToEmails = emails;
            await SendEmail(userEmailOptions);
        }

        // 2. send registration email confirmation email
        public async Task SendRegistrationConfirmationEmail(UserEmailOptions userEmailOptions)
        {
            // create new instance of email option 
            userEmailOptions.Subject = UpdatePlaceHolders("ExamRoom AI - Confirm Email", userEmailOptions.PlaceHolders);
            userEmailOptions.Body = UpdatePlaceHolders(GetEmailBody("confirmEmail"), userEmailOptions.PlaceHolders);
            await SendEmail(userEmailOptions);
        }

        // 3. Send forgot password link email
        public async Task SendForgotPasswordLinkEmail(UserEmailOptions userEmailOptions)
        {
            // create new instance of email option 
            userEmailOptions.Subject = UpdatePlaceHolders("ExamRoom AI - Forgot Password", userEmailOptions.PlaceHolders);
            userEmailOptions.Body = UpdatePlaceHolders(GetEmailBody("forgotPassword"), userEmailOptions.PlaceHolders);
            await SendEmail(userEmailOptions);
        }
        
        // 4. Send password changed success email
        public async Task SendPasswordChangedSuccessEmail(UserEmailOptions userEmailOptions)
        {
            // create new instance of email option 
            userEmailOptions.Subject = UpdatePlaceHolders("ExamRoom AI - Password Changed", userEmailOptions.PlaceHolders);
            userEmailOptions.Body = UpdatePlaceHolders(GetEmailBody("passwordChanged"), userEmailOptions.PlaceHolders);
            await SendEmail(userEmailOptions);
        }
        
        // 4. Send password changed success email
        public async Task SendInActiveUsersEmail(UserEmailOptions userEmailOptions)
        {
            // create new instance of email option 
            userEmailOptions.Subject = UpdatePlaceHolders("ExamRoom AI - Inactivity", userEmailOptions.PlaceHolders);
            userEmailOptions.Body = UpdatePlaceHolders(GetEmailBody("inactiveUser"), userEmailOptions.PlaceHolders);
            await SendEmail(userEmailOptions);
        }

        // method to generate the verification code 
        public string GenerateVerificationCode()
        {
            Random rand = new Random();
            int number = rand.Next(1001, 9999);
            // TODO: save the verification to the database 
            return number.ToString();
        }

        public async Task SendVerificationSMS(string phoneNumber, string verificationCode, string clientName)
        {
            var messsage = await MessageResource.CreateAsync(
                to: new Twilio.Types.PhoneNumber(phoneNumber),
                from: new Twilio.Types.PhoneNumber("+12762901606"),
                body: $"Hi {clientName}, your verification code is {verificationCode}",
                client: _client
                );
        }

    }
}
