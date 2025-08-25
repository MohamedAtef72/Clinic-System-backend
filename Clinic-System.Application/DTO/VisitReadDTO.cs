using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class VisitReadDTO
    {
        public int Id { get; set; }
        public int Price { get; set; }
        public string DoctorNotes { get; set; }
        public string Medicine { get; set; }
        public string VisitStatus { get; set; }

        public int AppointmentId { get; set; }
    }
}
