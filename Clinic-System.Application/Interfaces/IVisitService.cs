using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IVisitService
    {
        Task<IEnumerable<VisitReadDTO>> GetAllVisitsAsync();
        Task<VisitReadDTO> GetVisitByIdAsync(int id);
        Task<IEnumerable<VisitReadDTO>> GetVisitsByDoctorIdAsync(Guid doctorId);
        Task<IEnumerable<VisitReadDTO>> GetVisitsByPatientIdAsync(Guid patientId);
        Task AddVisitAsync(VisitCreateDTO visit);
        Task UpdateVisitAsync(VisitReadDTO visit);
        Task DeleteVisitAsync(int id);
    }
}
