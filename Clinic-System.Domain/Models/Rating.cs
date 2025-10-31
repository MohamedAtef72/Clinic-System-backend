using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Domain.Models
{
    public class Rating
    {
        public int Id { get; set; }
        [ForeignKey(nameof(Appointment))]
        public int AppointmentId { get; set; }
        [ForeignKey(nameof(Doctor))]
        public Guid DoctorId { get; set; }
        [ForeignKey(nameof(Patient))]
        public Guid PatientId { get; set; }

        public int Rate { get; set; } 
        public string? Comment { get; set; }

        // Navigation properties
        public Appointment Appointment { get; set; }
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
    }

}
