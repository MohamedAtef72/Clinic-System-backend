using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // Navigation
        public ApplicationUser User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}