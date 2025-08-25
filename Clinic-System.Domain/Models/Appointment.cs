using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.Domain.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Availability))]
        public int AvailabilityId { get; set; }
        public DoctorAvailability Availability { get; set; }

        [Required]
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string AppointmentStatus { get; set; }

        // Navigation
        public Patient Patient { get; set; }
        public Visit Visit { get; set; }
    }
}
