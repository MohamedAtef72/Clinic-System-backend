using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Navigation Properties
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public Receptionist Receptionist { get; set; }
    }
}