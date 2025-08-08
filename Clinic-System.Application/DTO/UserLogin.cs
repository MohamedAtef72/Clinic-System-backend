using System.ComponentModel.DataAnnotations;

namespace Clinic_System.Application.DTO
{
    public class UserLogin
    {
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}