using Clinic_System.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IAdminService
    {
        Task<DashboardDTO> GetDashboardInfo();

        Task<RecentActivityDataDTO> GetRecentActivityData();
    }
}
