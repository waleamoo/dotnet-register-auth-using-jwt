using System.ComponentModel.DataAnnotations;

namespace ExamRoomBackend.Models.DTO
{
    public class RegisterReqDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(30, ErrorMessage = "Firstname cannot be more than 30 characters")]
        
        public string FirstName { get; set; } = string.Empty;
        [Required]
        [StringLength(30, ErrorMessage = "Lastname cannot be more than 30 characters")]
        public string LastName { get; set; } = string.Empty;
        [Required, MinLength(6, ErrorMessage = "Please enter at least 6 characters for password")]
        public string Password { get; set; }
        [Required, Compare("Password")]
        public string PasswordConfirm { get; set; }

    }
}
