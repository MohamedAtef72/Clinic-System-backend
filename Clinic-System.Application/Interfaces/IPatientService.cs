using Clinic_System.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IPatientService
    {
        Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsAsync( string? searchName, int pageNumber, int pageSize);
        Task<PatientInfoDTO> GetPatientByIdAsync(Guid id);
        Task<PatientInfoDTO> GetPatientByUserIdAsync(string userId);
    }
}
