using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using CloudinaryDotNet.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly AppDbContext _db;

        public DoctorRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddDoctorAsync(Doctor newDoctor)
        {
            if (newDoctor != null)
            {
                await _db.Doctors.AddAsync(newDoctor);
                await SaveChanges();
            }
        }

        public async Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(string? searchName, string? gender, int? speciality, int pageNumber, int pageSize)
        {
            var query = _db.Doctors
                .AsNoTracking()
                        .Include(d => d.User)
                        .Include(d => d.Speciality)
                        .Where(d => !d.User.IsDeleted) 
                        .AsQueryable();

            if (!String.IsNullOrEmpty(searchName))
            {
                query = query 
                .Where(d => d.User.UserName.Contains(searchName));
            }

            if (!String.IsNullOrEmpty(gender))
            {
                query = query.Where(a => a.User.Gender == gender);
            }

            if (speciality.HasValue)
            {   
                query = query.Where(a => a.SpecialityId == speciality);
            }

            var totalCount = await query.CountAsync();

            var doctors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(doctor => new DoctorInfoDTO
                {
                    Id = doctor.Id.ToString(),
                    UserId = doctor.User.Id,
                    UserName = doctor.User.UserName,
                    Email = doctor.User.Email,
                    Country = doctor.User.Country,
                    Gender = doctor.User.Gender,
                    ImagePath = doctor.User.ImagePath,
                    DateOfBirth = doctor.User.DateOfBirth,
                    RegisterDate = doctor.User.RegisterDate,
                    SpecialityId = doctor.SpecialityId,
                    SpecialityName = doctor.Speciality.Name,
                }).ToListAsync();

            return (doctors, totalCount);
        }

        public async Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsWithDeletedAsync(string? searchName, string? gender, int? speciality, int pageNumber, int pageSize)
        {
            var query = _db.Doctors
                .AsNoTracking()
                        .IgnoreQueryFilters()
                        .Include(d => d.User)
                        .Include(d => d.Speciality)
                        .AsQueryable();

            if (!String.IsNullOrEmpty(searchName))
            {
                query = query
                .Where(d => d.User.UserName.Contains(searchName));
            }
            if (!String.IsNullOrEmpty(gender))
            {
                query = query.Where(a => a.User.Gender == gender);
            }

            if (speciality.HasValue)
            {
                query = query.Where(a => a.SpecialityId == speciality);
            }

            var totalCount = await query.CountAsync();

            var doctors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(doctor => new DoctorInfoDTO
                {
                    Id = doctor.Id.ToString(),
                    UserId = doctor.User.Id,
                    UserName = doctor.User.UserName,
                    Email = doctor.User.Email,
                    Country = doctor.User.Country,
                    Gender = doctor.User.Gender,
                    ImagePath = doctor.User.ImagePath,
                    DateOfBirth = doctor.User.DateOfBirth,
                    RegisterDate = doctor.User.RegisterDate,
                    SpecialityId = doctor.SpecialityId,
                    SpecialityName = doctor.Speciality.Name,
                    IsDeleted = doctor.User.IsDeleted,
                }).ToListAsync();

            return (doctors, totalCount);
        }

        public async Task<DoctorInfoDTO?> GetDoctorByIdAsync(Guid id)
        {
            var doctor = await _db.Doctors
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Include(d => d.User)
                .Include(d => d.Speciality)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (doctor == null) return null;

            return new DoctorInfoDTO
            {
                Id = doctor.Id.ToString(),
                UserId = doctor.User.Id,
                UserName = doctor.User.UserName,
                Email = doctor.User.Email,
                ConsulationPrice = doctor.ConsultationPrice,
                Country = doctor.User.Country,
                Gender = doctor.User.Gender,
                ImagePath = doctor.User.ImagePath,
                DateOfBirth = doctor.User.DateOfBirth,
                RegisterDate = doctor.User.RegisterDate,
                SpecialityId = doctor.SpecialityId,
                SpecialityName = doctor.Speciality?.Name,
            };
        }

        public async Task<DoctorInfoDTO?> GetDoctorByUserIdAsync(string userId)
        {
            var doctor = await _db.Doctors
                .AsNoTracking()
                .Include(d => d.User)
                .Include(d => d.Speciality)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (doctor == null) return null;

            return new DoctorInfoDTO
            {
                Id = doctor.Id.ToString(),
                UserId = doctor.User.Id,
                UserName = doctor.User.UserName,
                Email = doctor.User.Email,
                Country = doctor.User.Country,
                Gender = doctor.User.Gender,
                ImagePath = doctor.User.ImagePath,
                DateOfBirth = doctor.User.DateOfBirth,
                RegisterDate = doctor.User.RegisterDate,
                SpecialityId = doctor.SpecialityId,
                SpecialityName = doctor.Speciality?.Name,
            };
        }

        public async Task<bool> UpdateDoctorPriceAsync(Guid doctorId, int price)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return false;

            doctor.ConsultationPrice = price;

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string?> GetUserIdByDoctorIdAsync(Guid doctorId)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            return doctor?.UserId;
        }

        public async Task<Doctor?> GetByUserIdAsync(string userId, bool includeDeleted = false)
        {
            var query = _db.Doctors.AsQueryable();

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
