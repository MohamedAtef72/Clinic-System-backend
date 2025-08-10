using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class ReceptionistRegister : UserRegisterBase
    {
        [Required]
        public short ShiftStart { get; set; }

        [Required]
        public short ShiftEnd { get; set; }
    }
}
