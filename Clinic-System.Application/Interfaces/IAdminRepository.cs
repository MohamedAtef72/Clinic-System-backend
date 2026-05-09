using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IAdminRepository
    {
        Task<(int doctorsCount, int patientsCount, int appointmentCount)> GetInfo();
        Task<(int newPatientsNumber, int newAppointmentsNumber)> GetRecentData();
    }
}
