using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Clinic_System.Infrastructure.Repositories
{
    public class PatientRepository
    {
        private readonly AppDbContext _db;

        public PatientRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsAsync(string? searchName, int pageNumber, int pageSize)
        {
            var query = _db.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .AsQueryable();

            if(!String.IsNullOrEmpty(searchName))
            {
                query = query.Where(a => a.User.UserName.Contains(searchName));
            }

            var totalCount = await query.CountAsync();

            var patients = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(patient => new PatientInfoDTO
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
                }).ToListAsync();

            return (patients, totalCount);
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _db.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                .ToListAsync();
        }
        // Get Patient From DB
        public async Task<Patient> GetPatientByIdAsync(Guid id)
        {
            return await _db.Patients.Include(d => d.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Patient> GetPatientByUserIdAsync(string userId)
        {
            return await _db.Patients.Include(d => d.User).FirstOrDefaultAsync(e => e.UserId == userId);
        }

        public async Task AddPatientAsync(Patient newPatient)
        {
            if (newPatient != null)
            {
                await _db.Patients.AddAsync(newPatient);
                await _db.SaveChangesAsync();
            }
        }
        // Add Patient Async
        public async Task AddPatient(Patient newPatient)
        {
            if (newPatient != null)
            {
                await _db.Patients.AddAsync(newPatient);
                await _db.SaveChangesAsync();
            }
        }

        // Update Patient Async
        public async Task<IdentityResult> UpdatePatientAsync(string userId, UserEditProfile PatientEdit)
        {
            var patientFromDB = await GetPatientByIdAsync(Guid.Parse(userId));
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
