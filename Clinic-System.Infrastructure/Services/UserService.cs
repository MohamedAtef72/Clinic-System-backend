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
        // Soft Delete User (deactivate without removing from DB)
        public async Task<IdentityResult> DeleteUserWithRelatedDataAsync(string userId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return IdentityResult.Failed(new IdentityError { Description = "User not found" });

                var utcNow = DateTime.UtcNow;

                // Soft delete the user (deactivate)
                user.IsDeleted = true;
                user.DeletedAt = utcNow;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                    return result;

                // Revoke all refresh tokens
                var refreshTokens = await _db.RefreshTokens
                    .Where(rt => rt.UserId == userId)
                    .ToListAsync();
                if (refreshTokens.Any())
                {
                    _db.RefreshTokens.RemoveRange(refreshTokens);
                }

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

        //// Get All Users With Details From Each Table With Pagination 
        public async Task<(List<UserWithDetails> Users, int TotalCount)> GetAllUsersWithDetailsAsync(int pageNumber, int pageSize)
        {
            var query = from user in _db.Users
                        where !user.IsDeleted // FILTER: Exclude soft-deleted users
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
                    // Safely handle null Doctor/Patient for deleted users
                    SpecialityId = u.User.Doctor != null ? u.User.Doctor.SpecialityId : 0,
                    BloodType = u.User.Patient != null ? u.User.Patient.BloodType : string.Empty,
                    MedicalHistory = u.User.Patient != null ? u.User.Patient.MedicalHistory : string.Empty,
                    ShiftStart = u.User.Receptionist != null ? u.User.Receptionist.ShiftStart : null,
                    ShiftEnd = u.User.Receptionist != null ? u.User.Receptionist.ShiftEnd : null
                })
                .ToListAsync();

            return (pagedUsers, totalCount);
        }

    }
}
