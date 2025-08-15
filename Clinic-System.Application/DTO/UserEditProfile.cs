using Microsoft.AspNetCore.Http;

namespace Clinic_System.Application.DTO
{
    public class UserEditProfile
    {
        public string UserName { get; set; }
        public string Country { get; set; }
        public IFormFile Image { get; set; }
        public int? SpecialityId { get; set; }
        public string? MedicalHistory { get; set; }
        public short? ShiftStart { get; set; }
        public short? ShiftEnd { get; set; }
    }
}