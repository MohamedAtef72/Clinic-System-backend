using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Constant;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Clinic_System.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserRepository _userRepo;
        private readonly DoctorRepository _doctorRepo;
        private readonly IPatientService _patientService;
        private readonly PatientRepository _patientRepo;
        private readonly ReceptionistRepository _receptionistRepo;
        private readonly IUserService _userService;

        public UserController(UserManager<ApplicationUser> userManager, UserRepository userRepo, DoctorRepository doctorRepo, PatientRepository patientRepo, ReceptionistRepository receptionistRepo, IUserService userService , IPatientService patientService)
        {
            _userManager = userManager;
            _userRepo = userRepo;
            _doctorRepo = doctorRepo;
            _patientRepo = patientRepo;
            _receptionistRepo = receptionistRepo;
            _userService = userService;
            _patientService = patientService;
        }

        [HttpGet("UserProfile")]
        public async Task<IActionResult> UserProfile()
        {
            var userId = _userRepo.GetUserIdFromJwtClaims();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing authentication token" });

            var userFromDB = await _userRepo.GetUserByIdAsync(userId);
            if (userFromDB == null)
                return NotFound(new { message = "User not found" });

            var userRole = await _userRepo.GetUserRole();
            if (userRole == null || userRole.Count == 0)
                return Forbid("User has no assigned roles");

            object user;
            string message;

            if (userRole.Contains("Doctor", StringComparer.OrdinalIgnoreCase))
            {
                var doctor = await _doctorRepo.GetDoctorByUserIdAsync(userId);
                if (doctor == null)
                    return NotFound(new { message = "Doctor details not found" });

                var dto = _userRepo.MapBaseUser<DoctorInfoDTO>(userFromDB);
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

                var dto = _userRepo.MapBaseUser<PatientInfoDTO>(userFromDB);
                dto.BloodType = patient.BloodType;
                dto.MedicalHistory = patient.MedicalHistory;
                dto.UserId = patient.UserId;
                user = dto;
                message = "Patient Retrieved Successfully";
            }
            else if (userRole.Contains("Receptionist", StringComparer.OrdinalIgnoreCase))
            {
                var receptionist = await _receptionistRepo.GetReceptionistByUserIdAsync(userId);
                if (receptionist == null)
                    return NotFound(new { message = "Receptionist details not found" });

                var dto = _userRepo.MapBaseUser<ReceptionistInfoDTO>(userFromDB);
                dto.ShiftStart = receptionist.ShiftStart;
                dto.ShiftEnd = receptionist.ShiftEnd;
                dto.UserId = receptionist.UserId;
                user = dto;
                message = "Receptionist Retrieved Successfully";
            }
            else if (userRole.Contains("Admin", StringComparer.OrdinalIgnoreCase))
            {
                user = _userRepo.MapBaseUser<UserInfo>(userFromDB);
                message = "Admin Retrieved Successfully";
            }
            else
            {
                return Forbid("User role not authorized");
            }

            return Ok(new { message, user , role = userRole });
        }


        [HttpPut("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserEditProfile userEdit)
        {
            var userId = _userRepo.GetUserIdFromJwtClaims();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing authentication token" });

            var userFromDB = await _userRepo.GetUserByIdAsync(userId);
            if (userFromDB == null)
                return NotFound(new { message = "User not found" });

            // Update common fields
            var result = await _userRepo.UpdateUserAsync(userEdit,userId);

            if (!result.Succeeded)
            {
                return BadRequest(new { Message = "Update failed."});
            }

            // Get role
            var roles = await _userRepo.GetUserRole();
            if (roles == null || roles.Count == 0)
                return Forbid("User has no assigned roles");

            var role = roles.FirstOrDefault();

            // Role-specific update
            if (role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var resultDoctorEdit = await _doctorRepo.UpdateDoctorAsync(userId,userEdit);
                if (!resultDoctorEdit.Succeeded)
                {
                    return BadRequest(new { Message = "Update Doctor Failed" });
                }
            }
            else if (role.Equals("Patient", StringComparison.OrdinalIgnoreCase))
            {
                var resultPatientEdit = await _patientRepo.UpdatePatientAsync(userId, userEdit);
                if (!resultPatientEdit.Succeeded)
                {
                    return BadRequest(new { Message = "Update Patient Failed" });
                }
            }
            else if (role.Equals("Receptionist", StringComparison.OrdinalIgnoreCase))
            {
                var resultReceptionistEdit = await _receptionistRepo.UpdateReceptionistAsync(Guid.Parse(userId),userEdit);
                if (!resultReceptionistEdit.Succeeded)
                {
                    return BadRequest(new { Message = "Update Receptionist Failed" });
                }
            }

            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpDelete("DeleteProfile/{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteProfile(string id)
        {
            var result = await _userService.DeleteUserWithRelatedDataAsync(id);

            if (result.Succeeded)
                return Ok(new { message = "User deleted successfully" });

            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
        }

        [HttpGet("AllUsers")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> GetAllUsers(int pageNumber = 1, int pageSize = 5)
        {
            var (users, totalCount) = await _userService.GetAllUsersWithDetailsAsync(pageNumber, pageSize);

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Data = users
            });
        }

    }
}