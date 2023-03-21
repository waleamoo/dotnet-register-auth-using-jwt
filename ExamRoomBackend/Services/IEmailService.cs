using ExamRoomBackend.Models;

namespace ExamRoomBackend.Services
{
    public interface IEmailService
    {
        Task SendRegistrationConfirmationEmail(UserEmailOptions userEmailOptions);
        Task SendVerificationEmail(UserEmailOptions userEmailOptions);
        string GenerateVerificationCode();
        Task SendVerificationSMS(string phoneNumber, string verificationCode, string clientName);
        Task SendForgotPasswordLinkEmail(UserEmailOptions userEmailOptions);
        Task SendPasswordChangedSuccessEmail(UserEmailOptions userEmailOptions);
        Task SendInActiveUsersEmail(UserEmailOptions userEmailOptions);
    }
}