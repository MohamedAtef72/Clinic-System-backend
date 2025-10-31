using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class CreateAppointmentDTO
    {
        public Guid PatientId { get; set; }
        public int AvailabilityId { get; set; }
        public DateTime Date { get; set; }
        public string AppointmentStatus { get; set; }
    }
}
