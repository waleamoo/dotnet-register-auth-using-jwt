using ExamRoomBackend.Helpers;
using ExamRoomBackend.Models;
using ExamRoomBackend.Models.DTO;

namespace ExamRoomBackend.Services
{
    public interface IUserRepository
    {
        Task<User> Authenticate(string email, string password);
        void Register(string email, string password, string firstName, string lastName);
        Task<bool> UserAlreadyExists(string email);
        Task<User> GetUser(string email);
        Task<bool> ForgotPassword(string email);
        Task<bool> ResetPassword(ResetPasswordModel request);
        string CreateJWT(User user);
        Task<bool> DeleteCandidiate(int id);
        Task<bool> VerifyUser(string token);
        Task<bool> EmailInActiveUsers();
    }
}
