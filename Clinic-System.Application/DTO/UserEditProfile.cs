using Microsoft.AspNetCore.Http;

namespace Clinic_System.Application.DTO
{
    public class UserEditProfile
    {
        public string UserName { get; set; }
        public string Country { get; set; }
        public IFormFile? Image { get; set; }
        public string? MedicalHistory { get; set; }
        public TimeSpan? ShiftStart { get; set; }
        public TimeSpan? ShiftEnd { get; set; }
    }
}