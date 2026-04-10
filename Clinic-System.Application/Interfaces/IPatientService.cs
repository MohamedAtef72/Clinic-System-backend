using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IPatientService
    {
        Task<Patient> AddPatient(string userId, string bloodType, string medicalHistory);
        Task<IdentityResult> UpdatePatientAsync(string userId, UserEditProfile PatientEdit);
        Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsAsync( string? searchName, int pageNumber, int pageSize);
        Task<PatientInfoDTO> GetPatientByIdAsync(Guid id);
        Task<PatientInfoDTO> GetPatientByUserIdAsync(string userId);
        Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsWithDeletedAsync(string? searchName, int pageNumber, int pageSize);
        Task<Patient> EnsurePatientExistsOrRestoreAsync(string userId, string bloodType, string medicalHistory);
    }
}
