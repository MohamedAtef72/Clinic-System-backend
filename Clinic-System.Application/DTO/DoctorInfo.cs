using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class DoctorInfo : UserInfo
    {
        public int SpecialityId { get; set; }
        public string UserId { get; set; }
    }
}
