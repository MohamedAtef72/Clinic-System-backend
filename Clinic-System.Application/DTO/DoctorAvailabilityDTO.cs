using Clinic_System.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class DoctorAvailabilityDTO
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public DateTime? RecurrenceEndDate { get; set; }
        public bool IsBooked { get; set; }
    }
}
