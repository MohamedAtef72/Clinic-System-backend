using Microsoft.AspNetCore.Identity;
using Clinic_System.Domain.Constant;
using Clinic_System.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Clinic_System.Application.Interfaces;

namespace Clinic_System.Infrastructure.Services
{
    public class RoleSeederService : IRoleSeederService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<AdminSettings> _adminSettings;
        private readonly ILogger<RoleSeederService> _logger;

        public RoleSeederService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<AdminSettings> adminSettings,
            ILogger<RoleSeederService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _adminSettings = adminSettings;
            _logger = logger;
        }
        // Method Check If Role In DB Or Add It In DB
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Role.AllRoles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
        public async Task SeedRolesAndAdminAsync()
        {
            try
            {
                // Create Admin Users
                await CreateAdminUsersAsync();

                _logger.LogInformation("Roles and admin users seeded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding roles and admin users");
                throw;
            }
        }

        private async Task CreateAdminUsersAsync()
        {
            var admins = _adminSettings.Value.Admins;
            var defaultPassword = _adminSettings.Value.DefaultAdminPassword;

            foreach (var adminInfo in admins)
            {
                await CreateAdminUserAsync(adminInfo, defaultPassword);
            }
        }

        private async Task CreateAdminUserAsync(AdminUserSetting adminInfo, string password)
        {
            var existingUser = await _userManager.FindByEmailAsync(adminInfo.Email);

            if (existingUser == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminInfo.UserName,
                    Email = adminInfo.Email,
                    PhoneNumber = adminInfo.PhoneNumber,
                    Country = adminInfo.Country,
                    Gender = adminInfo.Gender,
                    DateOfBirth = adminInfo.DateOfBirth,
                    RegisterDate = adminInfo.RegisterDate,
                    ImagePath = adminInfo.ImagePath,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, Role.Admin);
                    _logger.LogInformation("Admin user {Email} created successfully", adminInfo.Email);
                }
                else
                {
                    _logger.LogError("Failed to create admin user {Email}: {Errors}",
                        adminInfo.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                if (!await _userManager.IsInRoleAsync(existingUser, Role.Admin))
                {
                    await _userManager.AddToRoleAsync(existingUser, Role.Admin);
                    _logger.LogInformation("Added admin role to existing user {Email}", adminInfo.Email);
                }
            }
        }
    }
}
