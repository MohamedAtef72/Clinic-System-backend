using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Clinic_System.Application.Interfaces
{
    public interface IRegisterService
    {
        public Task<(string? Error, ApplicationUser? User)> RegisterUserAsync(UserRegisterBase dto, IFormFile image, string role);
    }
}
