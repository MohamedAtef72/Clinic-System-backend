using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Clinic_System.Infrastructure.Repositories
{
    public class UserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoService _photoService;


        public UserRepository(IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, IPhotoService photoService)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _photoService = photoService;
        }

        // Get current user ID from JWT claims
        public string? GetUserIdFromJwtClaims()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
            if (claimsPrincipal == null)
                return null;

            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }

        // Get specific user by ID
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        // Get userRole

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
        // Update User Async
        public async Task<IdentityResult> UpdateUserAsync(UserEditProfile userEdit , string userId)
        {
            var userFromDB = await GetUserByIdAsync(userId);

            if (userFromDB == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            // Update common fields
            userFromDB.UserName = userEdit.UserName;
            userFromDB.Country = userEdit.Country;

            // Upload new image if provided
            if (userEdit.Image != null)
             {
                  var imageUrl = await _photoService.UploadImageAsync(userEdit.Image);
                  userFromDB.ImagePath = imageUrl;
             }
              var result = await _userManager.UpdateAsync(userFromDB);
              return result;
        }        
    }
}
