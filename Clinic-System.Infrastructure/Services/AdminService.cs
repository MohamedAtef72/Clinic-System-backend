using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class AdminService : IAdminService
    {
        private readonly AdminRepository _adminRepo;
        public AdminService(AdminRepository adminRepository)
        {
            _adminRepo = adminRepository;
        }
        public async Task<DashboardDTO> GetDashboardInfo()
        {
            var (doctorsCount , patientsCount , appointmentsCount) = await _adminRepo.GetInfo();
            
            if(doctorsCount != null || patientsCount != null || appointmentsCount != null)
            {
                return new DashboardDTO
                {
                    DoctorsCount = doctorsCount,
                    PatientsCount = patientsCount,
                    AppointmentsCount = appointmentsCount
                };
            }
            else
            {
                return null;
            }
        }

        public async Task<RecentActivityDataDTO> GetRecentActivityData()
        {
            var(patientsNumber , appointmentsNumber) = await _adminRepo.GetRecentData();

            var newModel = new RecentActivityDataDTO
            {
                newPatientsToday = patientsNumber,
                appointmentsToday = appointmentsNumber
            };

            return newModel;
        }
    }
}
