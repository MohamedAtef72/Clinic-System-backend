using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Constant;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Clinic_System.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IReceptionistService _receptionistService;
        private readonly IUserService _userService;
        private readonly ICacheService _cache;
        private readonly ILogger _logger;

        public UserController(IDoctorService doctorService, IPatientService patientService,IReceptionistService receptionistService, IUserService userService , Clinic_System.Application.Interfaces.ICacheService cache)
        {
            _doctorService = doctorService;
            _receptionistService = receptionistService;
            _userService = userService;
            _patientService = patientService;
            _cache = cache;
        }

        [HttpGet("UserProfile")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> UserProfile()
        {
            try
            {
                var userId = _userService.GetUserIdFromJwtClaims();
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid or missing authentication token" });

                var userRole = await _userService.GetUserRole();
                if (userRole == null || userRole.Count == 0)
                    return Forbid("User has no assigned roles");

                // Build cache key AFTER validating role
                var version = await _cache.GetVersionAsync($"user:profile:{userId}");
                var roleKey = string.Join("-", userRole);
                var cacheKey = $"user:profile:{userId}:{roleKey}:{version}";

                // Try to get from cache
                var cached = await _cache.GetAsync<dynamic>(cacheKey);
                if (cached is not null)
                {
                    return Ok(cached);
                }

                // Get user from database
                var userFromDB = await _userService.GetUserByIdAsync(userId);
                if (userFromDB == null)
                    return NotFound(new { message = "User not found" });

                object user;
                string message;

                // Build role-specific response
                if (userRole.Contains("Doctor", StringComparer.OrdinalIgnoreCase))
                {
                    var doctor = await _doctorService.GetDoctorByUserIdAsync(userId);
                    if (doctor == null)
                        return NotFound(new { message = "Doctor details not found" });

                    var dto = _userService.MapBaseUser<DoctorInfoDTO>(userFromDB);
                    dto.SpecialityId = doctor.SpecialityId;
                    dto.UserId = doctor.UserId;
                    dto.SpecialityName = doctor.SpecialityName;
                    user = dto;
                    message = "Doctor Retrieved Successfully";
                }
                else if (userRole.Contains("Patient", StringComparer.OrdinalIgnoreCase))
                {
                    var patient = await _patientService.GetPatientByUserIdAsync(userId);
                    if (patient == null)
                        return NotFound(new { message = "Patient details not found" });

                    var dto = _userService.MapBaseUser<PatientInfoDTO>(userFromDB);
                    dto.BloodType = patient.BloodType;
                    dto.MedicalHistory = patient.MedicalHistory;
                    dto.UserId = patient.UserId;
                    user = dto;
                    message = "Patient Retrieved Successfully";
                }
                else if (userRole.Contains("Receptionist", StringComparer.OrdinalIgnoreCase))
                {
                    var receptionist = await _receptionistService.GetReceptionistByUserIdAsync(userId);
                    if (receptionist == null)
                        return NotFound(new { message = "Receptionist details not found" });

                    var dto = _userService.MapBaseUser<ReceptionistInfoDTO>(userFromDB);
                    dto.ShiftStart = receptionist.ShiftStart;
                    dto.ShiftEnd = receptionist.ShiftEnd;
                    dto.UserId = receptionist.UserId;
                    user = dto;
                    message = "Receptionist Retrieved Successfully";
                }
                else if (userRole.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                {
                    user = _userService.MapBaseUser<UserInfo>(userFromDB);
                    message = "Admin Retrieved Successfully";
                }
                else
                {
                    return Forbid("User role not authorized");
                }

                // Build response and cache it
                var response = new { message, user, role = userRole };
                await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user profile", error = ex.Message });
            }
        }

        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserEditProfile userEdit)
        {
            var userId = _userService.GetUserIdFromJwtClaims();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing authentication token" });

            var userFromDB = await _userService.GetUserByIdAsync(userId);
            if (userFromDB == null)
                return NotFound(new { message = "User not found" });

            // Update common fields
            var result = await _userService.UpdateUserAsync(userEdit, userId);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    Message = "Update failed from user repository.",
                    Errors = result.Errors.Select(e => e.Description)
                });
            }

            // Get role
            var roles = await _userService.GetUserRole();
            if (roles == null || roles.Count == 0)
                return Forbid("User has no assigned roles");

            var role = roles.FirstOrDefault();

            // Role-specific update
            //if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            //{
            //    var resultDoctorEdit = await _doctorService.UpdateDoctorAsync(userId, userEdit);
            //    if (!resultDoctorEdit.Succeeded)
            //    {
            //        return BadRequest(new { Message = "Update Doctor Failed" });
            //    }
            //}
            if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var resultPatientEdit = await _patientService.UpdatePatientAsync(userId, userEdit);
                if (!resultPatientEdit.Succeeded)
                {
                    _logger.LogInformation("Update Patient Failed for UserId:Errors: {Errors}", string.Join(", ", resultPatientEdit.Errors.Select(e => e.Description)));
                    return BadRequest(new
                    {
                        Message = "Update Patient Failed from Patient Repository",
                        Errors = resultPatientEdit.Errors.Select(e => e.Description)
                    });
                }
            }
            else if (role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var resultReceptionistEdit = await _receptionistService.UpdateReceptionistAsync(userId, userEdit);
                if (!resultReceptionistEdit.Succeeded)
                {
                    return BadRequest(new { Message = "Update Receptionist Failed In Receptionist Repository", Errors = resultReceptionistEdit.Errors.Select(e => e.Description) });
                }
            }

            // Invalidate user profile cache
            await _cache.BumpVersionAsync($"user:profile:{userId}");
            await _cache.BumpVersionAsync("user:all");

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpDelete("DeleteProfile/{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteProfile(string id)
        {
            // Get user roles before deletion
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var userRoles = await _userService.GetUserRole();
            // Perform soft delete
            var result = await _userService.DeleteUserWithRelatedDataAsync(id);

            if (result.Succeeded)
            {
                // Invalidate cache for deleted doctor
                if (userRoles.Contains("Doctor", StringComparer.OrdinalIgnoreCase))
                {
                    await _cache.BumpVersionAsync("doctors:list");
                }

                // Invalidate cache for deleted patient if needed
                if (userRoles.Contains("Patient", StringComparer.OrdinalIgnoreCase))
                {
                    await _cache.BumpVersionAsync("patients:list");
                }

                // Invalidate user profile cache
                await _cache.BumpVersionAsync($"user:profile:{id}:{string.Join("-", userRoles)}");
                await _cache.BumpVersionAsync("user:all");

                return Ok(new { message = "User deleted successfully and cache invalidated" });
            }

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [HttpGet("AllUsers")]
        [Authorize(Roles = Role.Admin)]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 5)
        {
            List<UserWithDetails> users; int totalCount;
            var version = await _cache.GetVersionAsync($"user:all");
            var cacheKey = $"user:all:{pageNumber}:{pageSize}:{version}";
            var cached = await _cache.GetAsync<(List<UserWithDetails> Users, int TotalCount)>(cacheKey);
            if (cached.Users != null)
            {
                users = cached.Users;
                totalCount = cached.TotalCount;
            }
            else
            {
                var result = await _userService.GetAllUsersWithDetailsAsync(pageNumber, pageSize);
                users = result.Users;
                totalCount = result.TotalCount;
            }

            var response = new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = users
            };
            await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(10));
            return Ok(response);
        }

    }
}