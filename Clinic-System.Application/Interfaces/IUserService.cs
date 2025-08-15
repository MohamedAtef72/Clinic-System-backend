using Clinic_System.Application.DTO;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IUserService
    {
        Task<IdentityResult> DeleteUserWithRelatedDataAsync(string userId);
        Task<(List<UserWithDetails> Users, int TotalCount)> GetAllUsersWithDetailsAsync(int pageNumber, int pageSize);
    }
}
