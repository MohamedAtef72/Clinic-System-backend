using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class PatientRepository
    {
        private readonly AppDbContext _db;

        public PatientRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddPatient(Patient newPatient)
        {
            if (newPatient != null)
            {
                await _db.Patients.AddAsync(newPatient);
                await _db.SaveChangesAsync();
            }
        }

        // Get Patient From DB
        public async Task<Patient> GetPatientByIdAsync(string userId)
        {
            return await _db.Patients.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // Update Patient Async
        public async Task<IdentityResult> UpdatePatientAsync(string userId, UserEditProfile PatientEdit)
        {
            var patientFromDB = await GetPatientByIdAsync(userId);
            if (patientFromDB == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Patient not found." });
            }

            patientFromDB.MedicalHistory = PatientEdit.MedicalHistory;

            _db.Patients.Update(patientFromDB);
            var changes = await _db.SaveChangesAsync();

            if (changes > 0)
                return IdentityResult.Success;

            return IdentityResult.Failed(new IdentityError { Description = "No changes were made." });
        }
    }
}
