using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface ISpecialityRepository
    {
        Task<IList<Speciality>> GetAllAsync();
        Task<Speciality?> GetByIdAsync(int id);
        Task AddAsync(Speciality speciality);
        Task UpdateAsync(Speciality speciality);
        Task DeleteAsync(Speciality speciality);
        Task<bool> HasDoctorsAsync(int specialityId);
        Task<bool> SaveChangesAsync();
    }
}
