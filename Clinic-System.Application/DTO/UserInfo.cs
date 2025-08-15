using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.DTO
{
    public class UserInfo
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Country{ get; set; }
        public string Gender { get; set; }
        public string ImagePath { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public DateOnly RegisterDate { get; set; }

    }
}
