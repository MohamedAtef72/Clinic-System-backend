using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class UserWithDetails
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }

        // Extra info for Doctor
        public int? SpecialityId { get; set; }

        // Extra info for Patient
        public string BloodType { get; set; }
        public string MedicalHistory { get; set; }

        // Extra info for Receptionist
        public TimeSpan? ShiftStart { get; set; }
        public TimeSpan? ShiftEnd { get; set; }
    }
}
