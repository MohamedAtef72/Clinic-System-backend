using System.Collections.Generic;

namespace Clinic_System.Application.DTO
{
    public class DoctorsListDto
    {
        public List<DoctorInfoDTO> Doctors { get; set; }
        public int TotalCount { get; set; }
    }
}