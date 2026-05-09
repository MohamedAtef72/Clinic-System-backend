using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IUserRepository
    {
        public string? GetUserIdFromJwtClaims();
        public Task<ApplicationUser?> GetUserByIdAsync(string userId);
        public Task<List<string>> GetUserRole();
        public T MapBaseUser<T>(ApplicationUser user) where T : UserInfo, new();
        public Task<IdentityResult> UpdateUserAsync(UserEditProfile userEdit, string userId);
        public Task<IdentityResult> DeleteUserWithRelatedDataAsync(string userId);
        public Task<(List<UserWithDetails> Users, int TotalCount)> GetAllUsersWithDetailsAsync(int pageNumber, int pageSize);


    }
}
