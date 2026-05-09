using Microsoft.AspNetCore.Http;

namespace Clinic_System.Application.DTO
{
    public class UserEditProfile
    {
        public string UserName { get; set; }
        public string Country { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ImagePath { get; set; }
        public string ? BloodType { get; set; }
        public string? MedicalHistory { get; set; }
        public TimeSpan? ShiftStart { get; set; }
        public TimeSpan? ShiftEnd { get; set; }
    }
}