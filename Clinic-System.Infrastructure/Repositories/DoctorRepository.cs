using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Clinic_System.Infrastructure.Repositories
{
    public class DoctorRepository
    {
        private readonly AppDbContext _db;

        public DoctorRepository(AppDbContext db)
        {
            _db = db;
        }

        // Add Doctor
        public async Task AddDoctorAsync(Doctor newDoctor)
        {
            if (newDoctor != null)
            {
                await _db.Doctors.AddAsync(newDoctor);
                await _db.SaveChangesAsync();
            }
        }

        // Get All Doctors
        public async Task<(List<DoctorInfoDTO> Doctors, int TotalCount)> GetAllDoctorsAsync(string? searchName, int pageNumber, int pageSize)
        {
            var query = _db.Doctors
                        .Include(d => d.User)
                        .Include(d => d.Speciality)
                        .Include(d => d.Availabilities)
                        .AsQueryable();

            if (!String.IsNullOrEmpty(searchName))
            {
                query = query 
                .Where(d => d.User.UserName.Contains(searchName));
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
                    Availabilities = doctor.Availabilities
                        .Select(a => new DoctorAvailabilityDTO
                        {
                            Id = a.Id,
                            StartTime = a.StartTime,
                            EndTime = a.EndTime,
                            IsBooked = a.IsBooked
                        }).ToList()
                }).ToListAsync();

            return (doctors, totalCount);
        }

        // Get Doctor by Id
        public async Task<DoctorInfoDTO?> GetDoctorByIdAsync(Guid id)
        {
            var doctor = await _db.Doctors
                .Include(d => d.User)
                .Include(d => d.Speciality)
                .Include(d => d.Availabilities)
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
                Availabilities = doctor.Availabilities
                    .Select(a => new DoctorAvailabilityDTO
                    {
                        Id = a.Id,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        IsBooked = a.IsBooked
                    }).ToList()
            };
        }

        // Get Doctor By UserId
        public async Task<DoctorInfoDTO?> GetDoctorByUserIdAsync(string userId)
        {
            var doctor = await _db.Doctors
                .Include(d => d.User)
                .Include(d => d.Speciality)
                .Include(d => d.Availabilities)
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
                Availabilities = doctor.Availabilities
                    .Select(a => new DoctorAvailabilityDTO
                    {
                        Id = a.Id,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        IsBooked = a.IsBooked
                    }).ToList()
            };
        }

        // Update Doctor
        public async Task<IdentityResult> UpdateDoctorAsync(string userId, UserEditProfile doctorEdit)
        {
            var doctorFromDB = await _db.Doctors.FirstOrDefaultAsync(e => e.UserId == userId);
            if (doctorFromDB == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Doctor not found." });
            }

            _db.Doctors.Update(doctorFromDB);
            var changes = await _db.SaveChangesAsync();

            return changes > 0
                ? IdentityResult.Success
                : IdentityResult.Failed(new IdentityError { Description = "No changes were made." });
        }

        // Update Price
        public async Task<bool> UpdateDoctorPriceAsync(Guid doctorId, int price)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
                return false;

            doctor.ConsultationPrice = price;

            await _db.SaveChangesAsync();
            return true;
        }

        // Get the ApplicationUser Id for a given doctor Id
        public async Task<string?> GetUserIdByDoctorIdAsync(Guid doctorId)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            return doctor?.UserId;
        }
    }
}
