using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(AppDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        // Delete User From Specific Table And From ASP.NET USERS
        public async Task<IdentityResult> DeleteUserWithRelatedDataAsync(string userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return IdentityResult.Failed(new IdentityError { Description = "User not found" });

                var doctor = await _db.Doctors.FirstOrDefaultAsync(e => e.UserId == userId);
                if (doctor != null)
                    _db.Doctors.Remove(doctor);

                var patient = await _db.Patients.FirstOrDefaultAsync(e => e.UserId == userId);
                if (patient != null)
                    _db.Patients.Remove(patient);

                var receptionist = await _db.Receptionists.FirstOrDefaultAsync(e => e.UserId == userId);
                if (receptionist != null)
                    _db.Receptionists.Remove(receptionist);

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    return result;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return IdentityResult.Failed(new IdentityError { Description = ex.Message });
            }
        }

        // Get All Users With Details From Each Table With Pagination 
        public async Task<(List<UserWithDetails> Users, int TotalCount)> GetAllUsersWithDetailsAsync(int pageNumber, int pageSize)
        {
            var query = from user in _db.Users
                        join userRole in _db.UserRoles on user.Id equals userRole.UserId into ur
                        from userRole in ur.DefaultIfEmpty()
                        join role in _db.Roles on userRole.RoleId equals role.Id into r
                        from role in r.DefaultIfEmpty()
                        select new
                        {
                            User = user,
                            RoleName = role.Name
                        };

            var totalCount = await query.CountAsync();

            var pagedUsers = await query
                .OrderBy(u => u.User.UserName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserWithDetails
                {
                    Id = u.User.Id,
                    UserName = u.User.UserName,
                    Email = u.User.Email,
                    Role = u.RoleName,
                    SpecialityId = u.User.Doctor.SpecialityId,
                    BloodType = u.User.Patient.BloodType,
                    MedicalHistory = u.User.Patient.MedicalHistory,
                    ShiftStart = u.User.Receptionist.ShiftStart,
                    ShiftEnd = u.User.Receptionist.ShiftEnd
                })
                .ToListAsync();

            return (pagedUsers, totalCount);
        }

    }
}
