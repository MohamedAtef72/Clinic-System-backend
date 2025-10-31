using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Domain.Models
{
    public class DoctorAvailability
    {
        public int Id { get; set; }

        [ForeignKey(nameof(Doctor))]
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // recurrence
        public string? RecurrencePattern { get; set; }   // "None", "Weekly", "BiWeekly", ...
        public DateTime? RecurrenceEndDate { get; set; }
        public Guid? SeriesId { get; set; } 

        public bool IsBooked { get; set; }
    }
}
