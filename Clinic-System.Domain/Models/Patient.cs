using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.Domain.Models
{
    public class Patient
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        public string BloodType { get; set; }

        [Required]
        public string MedicalHistory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Navigation
        public ApplicationUser User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}