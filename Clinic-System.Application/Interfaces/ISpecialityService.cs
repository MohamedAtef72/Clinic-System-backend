using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface ISpecialityService
    {
        Task<IList<Speciality>> GetAllAsync();
        Task<Speciality?> GetByIdAsync(int id);
        Task<Speciality> CreateAsync(Speciality speciality);
        Task<bool> UpdateAsync(int id, Speciality speciality);
        Task<bool> DeleteAsync(int id);
    }
}
