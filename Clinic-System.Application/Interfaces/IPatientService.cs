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
        Task<(List<PatientSummaryDTO> Patients, int TotalCount)> GetAllPatientsAsync(int pageNumber, int pageSize);
        Task<PatientSummaryDTO> GetPatientByIdAsync(string id);
    }
}
