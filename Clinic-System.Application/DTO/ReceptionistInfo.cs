using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class ReceptionistInfo : UserInfo
    {
        public string UserId { get; set; }
        public short ShiftStart { get; set; }
        public short ShiftEnd { get; set; }
    }
}
