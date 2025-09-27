using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class PatientInfoDTO : UserInfo
    {
        public string UserId { get; set; }
        public string BloodType { get; set; }
        public string MedicalHistory { get; set; }
    }
}
