using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class PatientSummaryDTO
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public string BloodType { get; set; }
        public string MedicalHistory { get; set; }
    }
}
