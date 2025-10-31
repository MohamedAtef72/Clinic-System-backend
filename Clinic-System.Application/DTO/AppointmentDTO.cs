using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public int? VisitId  { get; set; }
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public int AvailabilityId { get; set; }
        public DateTime Date { get; set; }
        public string AppointmentStatus { get; set; }
        public string MedicalHistory { get; set; }
    }
}
