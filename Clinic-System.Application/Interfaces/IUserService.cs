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
    public interface IUserService
    {
        string? GetUserIdFromJwtClaims();
        Task<ApplicationUser?> GetUserByIdAsync(string userId);
        Task<List<string>> GetUserRole();
        T MapBaseUser<T>(ApplicationUser user) where T : UserInfo, new();
        Task<IdentityResult> UpdateUserAsync(UserEditProfile userEdit, string userId);
        Task<IdentityResult> DeleteUserWithRelatedDataAsync(string userId);
        Task<(List<UserWithDetails> Users, int TotalCount)> GetAllUsersWithDetailsAsync(int pageNumber, int pageSize);
    }
}
