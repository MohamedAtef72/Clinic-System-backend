using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IVisitRepository
    {
        Task<IEnumerable<Visit>> GetAllAsync();
        Task<Visit> GetByIdAsync(int id);
        Task<IEnumerable<Visit>> GetVisitsByDoctorIdAsync(Guid doctorId);
        Task<IEnumerable<Visit>> GetVisitsByPatientIdAsync(Guid patientId);
        Task AddAsync(Visit visit);
        Task UpdateAsync(Visit visit);
        Task DeleteAsync(int id);
    }
}
