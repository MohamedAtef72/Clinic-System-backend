using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clinic_System.Domain.Common;

namespace Clinic_System.Domain.Models
{
    public class Patient : PersonInfo
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        [Required]
        public string BloodType { get; set; }

        [Required]
        public string MedicalHistory { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}