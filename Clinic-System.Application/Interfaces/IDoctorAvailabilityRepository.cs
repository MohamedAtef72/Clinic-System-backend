using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IDoctorAvailabilityRepository
    {
        Task AddAsync(DoctorAvailability availability);
        Task<IEnumerable<DoctorAvailability>> GetAllAsync();
        Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(Guid doctorId);
        Task<DoctorAvailability?> GetByIdAsync(int id);
        Task UpdateAsync(DoctorAvailability availability);
        Task DeleteAsync(int id);
        //Task<List<DoctorAvailability>> GetUnbookedByDoctorIdAsync(Guid doctorId);
        //void RemoveRange(List<DoctorAvailability> availabilities);
        Task DeleteUnbookedByDoctorIdAsync(Guid doctorId);
    }
}
