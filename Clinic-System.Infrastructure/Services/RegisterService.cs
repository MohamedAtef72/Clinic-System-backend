using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        public async Task<(string? Error, ApplicationUser? existingUser)> RegisterUserAsync(UserRegisterBase dto, IFormFile image, string role)
        {
            var email = dto.Email;
            var existingUser = await _userManager.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
            {
                if (existingUser.IsDeleted)
                {
                    // Reactivate user
                    existingUser.IsDeleted = false;
                    existingUser.DeletedAt = null;

                    if (dto.Password != dto.ConfirmPassword)
                        return ("Passwords do not match", null);

                    var url = await _photoService.UploadImageAsync(image);

                    existingUser.UserName = dto.UserName;
                    existingUser.Email = dto.Email;
                    existingUser.PhoneNumber = dto.PhoneNumber;
                    existingUser.Country = dto.Country;
                    existingUser.Gender = dto.Gender;
                    existingUser.DateOfBirth = dto.DateOfBirth;
                    existingUser.RegisterDate = dto.RegisterDate;
                    existingUser.ImagePath = url;

                    // Reset password
                    var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
                    var resetResult = await _userManager.ResetPasswordAsync(existingUser, token, dto.Password);
                    if (!resetResult.Succeeded)
                    {
                        var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                        return (errors, null);
                    }

                    await _userManager.UpdateAsync(existingUser);

                    // Ensure user is in the correct role
                    var roles = await _userManager.GetRolesAsync(existingUser);
                    if (!roles.Contains(role))
                        await _userManager.AddToRoleAsync(existingUser, role);

                    return (null, existingUser);
                }
                else
                {
                    // Email already in use by active user
                    return ("Email already in use", null);
                }
            }

            if (dto.Password != dto.ConfirmPassword)
                return ("Passwords do not match", null);

            var urlNew = await _photoService.UploadImageAsync(image);

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Country = dto.Country,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                RegisterDate = dto.RegisterDate,
                ImagePath = urlNew
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ("Password is not strong", null);

            await _userManager.AddToRoleAsync(user, role);
            return (null, user);
        }
    }
}