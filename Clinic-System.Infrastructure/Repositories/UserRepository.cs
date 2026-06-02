using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Clinic_System.Infrastructure.Repositories
{
    public class UserRepository: IUserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoService _photoService;
        private readonly AppDbContext _db;


        public UserRepository(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IPhotoService photoService,AppDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _photoService = photoService;
            _db = db;
        }
        public string? GetUserIdFromJwtClaims()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal == null)
                return null;

            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }
        public async Task<List<string>> GetUserRole()
        {
            var userId = GetUserIdFromJwtClaims();
            if (string.IsNullOrEmpty(userId))
                return new List<string>(); 

            var user = await GetUserByIdAsync(userId);
            if (user == null)
                return new List<string>();

            var userRoles = await _userManager.GetRolesAsync(user);
            return userRoles.ToList();
        }
        // Mapper Method For ADD Sharing Information
        public T MapBaseUser<T>(ApplicationUser user) where T : UserInfo, new()
        {
            return new T
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Country = user.Country,
                Gender = user.Gender,
                ImagePath = user.ImagePath,
                DateOfBirth = user.DateOfBirth,
                RegisterDate = user.RegisterDate
            };
        }
        public async Task<IdentityResult> UpdateUserAsync(UserEditProfile userEdit , string userId)
        {
                var userFromDB = await GetUserByIdAsync(userId);

                if (userFromDB == null)
                {
                    return IdentityResult.Failed(new IdentityError { Description = "User not found." });
                }

                // Update common fields
                if (userEdit.UserName != null && userEdit.UserName != userFromDB.UserName)
                {
                    userFromDB.UserName = userEdit.UserName;
                }
                if (userEdit.Country != null && userEdit.Country != userFromDB.Country)
                {
                    userFromDB.Country = userEdit.Country;
                }
                // Upload new image if provided
                if (userEdit.ImagePath != null && userEdit.ImagePath != userFromDB.ImagePath)
                {
                    userFromDB.ImagePath = userEdit.ImagePath;
                }
                // UserManager.UpdateAsync() calls SaveChangesAsync() internally
                return await _userManager.UpdateAsync(userFromDB);
        }
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
