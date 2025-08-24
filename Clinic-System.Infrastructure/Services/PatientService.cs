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

        public async Task<(List<PatientSummaryDTO> Patients, int TotalCount)> GetAllPatientsAsync(int pageNumber, int pageSize)
        {
            return await _patientRepository.GetAllPatientsAsync(pageNumber, pageSize);
        }

        public async Task<PatientSummaryDTO> GetPatientByIdAsync(string id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null) return null;

            return new PatientSummaryDTO
            {
                Id = patient.Id,
                UserId = patient.UserId,
                BloodType = patient.BloodType,
                MedicalHistory = patient.MedicalHistory
            };
        }
    }
}
