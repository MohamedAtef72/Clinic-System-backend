using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;

namespace Clinic_System.Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly PatientRepository _patientRepository;

        public PatientService(PatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsAsync( string? searchName, int pageNumber, int pageSize)
        {
            return await _patientRepository.GetAllPatientsAsync( searchName,pageNumber, pageSize);
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
    }
}
