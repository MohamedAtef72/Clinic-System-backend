using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Clinic_System.Infrastructure.Services
{
    public class RegisterService : IRegisterService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPhotoService _photoService;
        private readonly ILogger<RegisterService> _logger;

        public RegisterService(UserManager<ApplicationUser> userManager, IPhotoService photoService, ILogger<RegisterService> logger)
        {
            _userManager = userManager;
            _photoService = photoService;
            _logger = logger;
        }

        public async Task<(string? Error, ApplicationUser? existingUser)> RegisterUserAsync(UserRegisterBase dto, string role)
        {
            var email = dto.Email;

            // Track: Email Check
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
                    {
                        _logger.LogWarning("[REGISTER_SERVICE] Password mismatch during reactivation");
                        return ("Passwords do not match", null);
                    }


                    existingUser.UserName = dto.UserName;
                    existingUser.Email = dto.Email;
                    existingUser.PhoneNumber = dto.PhoneNumber;
                    existingUser.Country = dto.Country;
                    existingUser.Gender = dto.Gender;
                    existingUser.DateOfBirth = dto.DateOfBirth;
                    existingUser.RegisterDate = dto.RegisterDate;
                    existingUser.ImagePath = dto.ImagePath;

                    // Track: Password Reset
                    var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);

                    var resetResult = await _userManager.ResetPasswordAsync(existingUser, token, dto.Password);

                    if (!resetResult.Succeeded)
                    {
                        var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                        _logger.LogError("[REGISTER_SERVICE] Password reset failed: {Errors}", errors);
                        return (errors, null);
                    }

                    // Track: User Update
                    await _userManager.UpdateAsync(existingUser);

                    // Ensure user is in the correct role
                    var roles = await _userManager.GetRolesAsync(existingUser);

                    if (!roles.Contains(role))
                    {
                        _logger.LogInformation("[REGISTER_SERVICE] Adding role {Role} to user", role);
                        await _userManager.AddToRoleAsync(existingUser, role);
                    }

                    return (null, existingUser);
                }
                else
                {
                    _logger.LogWarning("[REGISTER_SERVICE] Email already in use by active user");
                    return ("Email already in use", null);
                }
            }

            if (dto.Password != dto.ConfirmPassword)
            {
                _logger.LogWarning("[REGISTER_SERVICE] Password mismatch for new user");
                return ("Passwords do not match", null);
            }

            // Track: Image Upload for New User
            //var urlNew = await _photoService.UploadImageAsync(dto.Image);
            //_logger.LogInformation("Image upload for new user: {Time}ms", sw.ElapsedMilliseconds);
            //sw.Restart();

            var user = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Country = dto.Country,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                RegisterDate = dto.RegisterDate,
                ImagePath = dto.ImagePath
            };

            // Track: User Creation
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                _logger.LogError("[REGISTER_SERVICE] User creation failed");
                return ("Password is not strong", null);
            }

            // Track: Role Assignment
            await _userManager.AddToRoleAsync(user, role);

            return (null, user);
        }
    }
}