using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class PatientRegisterDTO : UserRegisterBase
    {
        public string BloodType { get; set; }
        [Required]
        public string MedicalHistory { get; set; }
    }
}
