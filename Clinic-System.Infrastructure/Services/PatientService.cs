using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Clinic_System.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsAsync( string? searchName, string? gender ,int pageNumber, int pageSize)
        {
            return await _patientRepository.GetAllPatientsAsync( searchName,gender,pageNumber, pageSize);
        }

        public async Task<PatientInfoDTO> GetPatientByIdAsync(Guid id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null) return null;

            return new PatientInfoDTO
            {
                Id = patient.Id.ToString(),
                UserName = patient.User.UserName,
                Email = patient.User.Email,
                Country = patient.User.Country,
                Gender = patient.User.Gender,
                ImagePath = patient.User.ImagePath,
                DateOfBirth = patient.User.DateOfBirth,
                RegisterDate = patient.User.RegisterDate,
                UserId = patient.User.Id,
                BloodType = patient.BloodType,
                MedicalHistory = patient.MedicalHistory,
            };
        }
        public async Task<PatientInfoDTO> GetPatientByUserIdAsync(string userId)
        {
            var patient = await _patientRepository.GetPatientByUserIdAsync(userId);
            if (patient == null) return null;

            return new PatientInfoDTO
            {
                Id = patient.Id.ToString(),
                UserName = patient.User.UserName,
                Email = patient.User.Email,
                Country = patient.User.Country,
                Gender = patient.User.Gender,
                ImagePath = patient.User.ImagePath,
                DateOfBirth = patient.User.DateOfBirth,
                RegisterDate = patient.User.RegisterDate,
                UserId = patient.User.Id,
                BloodType = patient.BloodType,
                MedicalHistory = patient.MedicalHistory,
            };
        }
        public async Task<Patient>GetPatientWithID(Guid id)
        {
            return await _patientRepository.GetPatientByIdAsync(id);
        }
        public async Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsWithDeletedAsync(string? searchName, string? gender, int pageNumber, int pageSize)
        {
            return await _patientRepository.GetAllPatientsWithDeletedAsync(searchName, gender, pageNumber, pageSize);
        }

        public async Task<Patient> EnsurePatientExistsOrRestoreAsync(string userId, string bloodType, string medicalHistory)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId, includeDeleted: true);

            if (patient != null)
            {
                // Business logic
                patient.BloodType = bloodType;
                patient.MedicalHistory = medicalHistory;

                await _patientRepository.SaveChanges();

                return patient;
            }

            return await AddPatient(userId, bloodType, medicalHistory);
        }

        public async Task<Patient> AddPatient(string userId, string bloodType, string medicalHistory)
        {
            var patient = new Patient
            {
                UserId = userId,
                BloodType = bloodType,
                MedicalHistory = medicalHistory
            };

            await _patientRepository.AddPatient(patient);
            return patient;
        }

        public async Task<IdentityResult> UpdatePatientAsync(string userId, UserEditProfile patientEdit)
            => await _patientRepository.UpdatePatientAsync(userId, patientEdit);
    }
}
