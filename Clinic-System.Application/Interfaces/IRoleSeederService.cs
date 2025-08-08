using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IRoleSeederService
    {
        Task SeedRolesAndAdminAsync();
    }
}
