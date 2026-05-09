using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class PatientsListDTO
    {
        public List<PatientInfoDTO> Patients { get; set; }
        public int TotalCount { get; set; }
    }
}
