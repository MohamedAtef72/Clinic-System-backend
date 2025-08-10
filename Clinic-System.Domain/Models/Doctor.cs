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
    public class Doctor : PersonInfo
    {
        [Key]
        public string Id { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public string UserId { get; set; }

        [Required]
        [ForeignKey(nameof(Speciality))]
        public int SpecialityId { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
        public Speciality Speciality { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
        public ICollection<DoctorAvailability> Availabilities { get; set; }


    }
}
