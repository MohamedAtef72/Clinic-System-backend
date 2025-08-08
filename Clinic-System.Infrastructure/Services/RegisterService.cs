using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Clinic_System.Infrastructure.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoService _photoService;

        public RegisterService(UserManager<ApplicationUser> userManager, IPhotoService photoService)
        {
            _userManager = userManager;
            _photoService = photoService;
        }

        public async Task<(string? Error, ApplicationUser? User)> RegisterUserAsync(UserRegisterBase dto, IFormFile image, string role)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return ("Email already exists", null);

            if (dto.Password != dto.ConfirmPassword)
                return ("Passwords do not match", null);

            var url = await _photoService.UploadImageAsync(image);
            if (url == null)
                return ("Image upload failed", null);

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Country = dto.Country,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                RegisterDate = dto.RegisterDate,
                ImagePath = url
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ("Password is not strong", null);

            await _userManager.AddToRoleAsync(user, role);
            return (null, user);
        }
    }
}
