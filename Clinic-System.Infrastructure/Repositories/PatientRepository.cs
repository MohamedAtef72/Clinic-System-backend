using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace Clinic_System.Infrastructure.Repositories
{
    public class PatientRepository: IPatientRepository
    {
        private readonly AppDbContext _db;

        public PatientRepository(AppDbContext db)
        {
            _db = db;
        }
        public async Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsAsync(string? searchName, string? gender, int pageNumber, int pageSize)
        {
            var query = _db.Patients
                .AsNoTracking()
                .Include(p => p.User)
                .AsQueryable();

            if(!String.IsNullOrEmpty(searchName))
            {
                query = query.Where(a => a.User.UserName.Contains(searchName));
            }

            if(!String.IsNullOrEmpty(gender))
            {
                query = query.Where(a => a.User.Gender == gender);
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
                .AsNoTracking()
                .Include(p => p.User)
                .ToListAsync();
        }
        public async Task<Patient> GetPatientByIdAsync(Guid id)
        {
            return await _db.Patients
                .AsNoTracking()
                        .IgnoreQueryFilters()
                        .Include(p => p.User)
                        .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<Patient> GetPatientByUserIdAsync(string userId)
        {
            return await _db.Patients.Include(d => d.User).FirstOrDefaultAsync(e => e.UserId == userId);
        }
        public async Task AddPatient(Patient newPatient)
        {
            if (newPatient != null)
            {
                await _db.Patients.AddAsync(newPatient);
                await _db.SaveChangesAsync();
            }
        }
        public async Task<IdentityResult> UpdatePatientAsync(string userId, UserEditProfile PatientEdit)
        {
            var patientFromDB = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patientFromDB == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Patient not found." });
            }
            bool isUpdated = false;
            if (PatientEdit.BloodType != null && patientFromDB.BloodType != PatientEdit.BloodType)
            {
                patientFromDB.BloodType = PatientEdit.BloodType;
                isUpdated = true;
            }
            if (PatientEdit.MedicalHistory != null && patientFromDB.MedicalHistory != PatientEdit.MedicalHistory)
            {
                patientFromDB.MedicalHistory = PatientEdit.MedicalHistory;
                isUpdated = true;
            }

            if (!isUpdated)
            {
                return IdentityResult.Failed(new IdentityError { Description = "No changes detected." });
            }

            await _db.SaveChangesAsync();

            return IdentityResult.Success;
        }
        public async Task<(List<PatientInfoDTO> Patients, int TotalCount)> GetAllPatientsWithDeletedAsync(string? searchName, string? gender, int pageNumber, int pageSize)
        {
            var query = _db.Patients
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(p => p.User)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchName))
            {
                query = query.Where(a => a.User.UserName.Contains(searchName));
            }
            if (!String.IsNullOrEmpty(gender))
            {
                query = query.Where(a => a.User.Gender == gender);
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
                    IsDeleted = patient.User.IsDeleted

                }).ToListAsync();

            return (patients, totalCount);
        }
        public async Task<Patient?> GetByUserIdAsync(string userId, bool includeDeleted = false)
        {
            var query = _db.Patients.AsQueryable();

            if (includeDeleted)
                query = query.IgnoreQueryFilters();

            return await query.FirstOrDefaultAsync(d => d.UserId == userId);
        }
        public async Task SaveChanges()
        {
            await _db.SaveChangesAsync();
        }
    }
}
