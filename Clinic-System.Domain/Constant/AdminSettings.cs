using Clinic_System.Domain.Models;

namespace Clinic_System.Domain.Constant
{
    public class AdminSettings
    {
        public string DefaultAdminPassword { get; set; }
        public List<AdminUserSetting> Admins { get; set; }
    }
}
