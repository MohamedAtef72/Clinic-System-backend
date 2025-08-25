using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.Domain.Models
{
    public class Visit
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Appointment))]
        public int AppointmentId { get; set; }

        [Required]
        public int Price { get; set; }

        [Required]
        public string DoctorNotes { get; set; }

        [Required]
        public string Medicine { get; set; }

        [Required]
        public string VisitStatus { get; set; }

        // Navigation
        public Appointment Appointment { get; set; }
    }
}
