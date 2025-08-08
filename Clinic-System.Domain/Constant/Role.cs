using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Domain.Constant
{
    public class Role
    {
        public const string Admin = "Admin";
        public const string Patient = "Patient";
        public const string Doctor = "Doctor";
        public const string Receptionist = "Receptionist";
        public static List<string> AllRoles => new List<string> { Admin, Doctor, Patient, Receptionist };
    }
}
