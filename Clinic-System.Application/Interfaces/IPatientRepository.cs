using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IPatientRepository
    {
        Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsAsync(string? searchName, int pageNumber, int pageSize);
        Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsWithDeletedAsync(string? searchName, int pageNumber, int pageSize);
        Task<Patient> GetPatientByIdAsync(Guid id);
        Task<Patient> GetPatientByUserIdAsync(string userId);
        Task AddPatient(Patient newPatient);
        Task<IdentityResult> UpdatePatientAsync(string userId, UserEditProfile patientEdit);
        Task<Patient?> GetByUserIdAsync(string userId, bool includeDeleted);
        Task SaveChanges();
    }
}
