using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ImagePath { get; set; }
        [Required]
        public string Country { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public DateOnly DateOfBirth { get; set; }
        [Required]
        public DateOnly RegisterDate { get; set; }
        public ICollection<RefreshToken>? RefreshTokens { get; set; } = new List<RefreshToken>();
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation Properties
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Receptionist Receptionist { get; set; }
    }
}