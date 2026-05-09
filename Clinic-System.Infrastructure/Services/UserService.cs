using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IPhotoService _photoService;

        public UserService(IUserRepository userRepository,IHttpContextAccessor httpContextAccessor, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
            _photoService = photoService;
        }

        public string? GetUserIdFromJwtClaims()
        {
            var value = _userRepository.GetUserIdFromJwtClaims();
            return value;
        }

        // Get specific user by ID
        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        // Get user roles
        public async Task<List<string>> GetUserRole()
        {
            var userRoles = await _userRepository.GetUserRole();
            return userRoles;
        }

        // Mapper Method For ADD Sharing Information
        public T MapBaseUser<T>(ApplicationUser user) where T : UserInfo, new()
        {
            var res = _userRepository.MapBaseUser<T>(user);
            return res;
        }

        // Update User Async
        public async Task<IdentityResult> UpdateUserAsync(UserEditProfile userEdit, string userId)
        {
            var result = await _userRepository.UpdateUserAsync(userEdit, userId);
            return result;
        }

        // Soft Delete User (deactivate without removing from DB)
        public async Task<IdentityResult> DeleteUserWithRelatedDataAsync(string userId)
        {
            return await _userRepository.DeleteUserWithRelatedDataAsync(userId);
        }

        // Get All Users With Details From Each Table With Pagination 
        public async Task<(List<UserWithDetails> Users, int TotalCount)> GetAllUsersWithDetailsAsync(int pageNumber, int pageSize)
        {
            return await _userRepository.GetAllUsersWithDetailsAsync(pageNumber, pageSize);
        }
    }
}
