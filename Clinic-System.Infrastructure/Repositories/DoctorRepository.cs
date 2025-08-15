using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public async Task AddDoctor(Doctor newDoctor)
        {
            if (newDoctor != null)
            {
                await _db.Doctors.AddAsync(newDoctor);
                await _db.SaveChangesAsync();
            }
        }

        // Get Doctor From DB
        public async Task<Doctor> GetDoctorByIdAsync(string userId)
        {
            return await _db.Doctors.FirstOrDefaultAsync(e => e.UserId == userId);
        }

        // Update Doctor Async
        public async Task<IdentityResult> UpdateDoctorAsync(string userId, UserEditProfile doctorEdit)
        {
            var doctorFromDB = await GetDoctorByIdAsync(userId);
            if (doctorFromDB == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Doctor not found." });
            }

            doctorFromDB.SpecialityId = doctorEdit.SpecialityId ?? doctorFromDB.SpecialityId;

            _db.Doctors.Update(doctorFromDB);
            var changes = await _db.SaveChangesAsync();

            if (changes > 0)
                return IdentityResult.Success;

            return IdentityResult.Failed(new IdentityError { Description = "No changes were made." });
        }
    }
}