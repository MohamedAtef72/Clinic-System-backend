using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public  class DoctorAvailabilityCreateDTO
    {
        public Guid DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? RecurrencePattern { get; set; }  // "None", "Weekly", "BiWeekly"
        public DateTime? RecurrenceEndDate { get; set; }
    }
}
