using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class DoctorInfoDTO : UserInfo
    {
        // From Users Table
        public string UserId { get; set; }
        public int SpecialityId { get; set; }
        public int ConsulationPrice { get; set; }
        public string SpecialityName { get; set; }
        public List<DoctorAvailabilityDTO> Availabilities { get; set; }
        = new List<DoctorAvailabilityDTO>();
    }
}
