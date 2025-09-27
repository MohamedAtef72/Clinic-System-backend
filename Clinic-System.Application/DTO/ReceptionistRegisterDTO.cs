using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class ReceptionistRegisterDTO : UserRegisterBase
    {
        [Required]
        public TimeSpan ShiftStart { get; set; }

        [Required]
        public TimeSpan ShiftEnd { get; set; }
    }
}
