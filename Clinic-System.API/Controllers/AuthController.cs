using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Repositories;
using Clinic_System.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DoctorRepository doctorRepository;
        private readonly PatientRepository patientRepository;
        private readonly ReceptionistRepository receptionistRepository;
        private readonly IRegisterService _registerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMailingServices _mailingServices;


        public AuthController(
            DoctorRepository doctorRepository,
            PatientRepository patientRepository,
            ReceptionistRepository receptionistRepository,
            IRegisterService registerService,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IAuthService authService,
            ILogger<AuthController> logger,
            IMailingServices mailingServices
            )
        {
            this.doctorRepository = doctorRepository;
            this.patientRepository = patientRepository;
            this.receptionistRepository = receptionistRepository;
            _registerService = registerService;
            _userManager = userManager;
            _config = config;
            _authService = authService;
            _logger = logger;
            _mailingServices = mailingServices;
        }

        [HttpPost("DoctorRegister")]
        public async Task<IActionResult> DoctorRegister([FromForm] DoctorRegister doctorRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            var (error, user) = await _registerService.RegisterUserAsync(doctorRegister, doctorRegister.Image, "Doctor");
            if (error != null)
            {
                ModelState.AddModelError("Register", error);
                return BadRequest(ModelState);
            }

            var doctor = new Doctor
            {
                UserId = user.Id,
                SpecialityId = doctorRegister.SpecialityId
            };

            await doctorRepository.AddDoctor(doctor);

            return Ok("Doctor Registered Successfully");
        }

        [HttpPost("PatientRegister")]
        public async Task<IActionResult> PatientRegister([FromForm] PatientRegister patientRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            var (error, user) = await _registerService.RegisterUserAsync(patientRegister, patientRegister.Image, "Patient");

            if (error != null)
            {
                ModelState.AddModelError("Register", error);
                return BadRequest(ModelState);
            }

            var patient = new Patient
            {
                UserId = user.Id,
                BloodType = patientRegister.BloodType,
                MedicalHistory = patientRegister.MedicalHistory
            };

            await patientRepository.AddPatient(patient);

            return Ok("Patient Registered Successfully");
        }

        [HttpPost("ReceptionRegister")]
        public async Task<IActionResult> ReceptionistRegister([FromForm] ReceptionistRegister receptionistRegister)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid data");

            var (error, user) = await _registerService.RegisterUserAsync(receptionistRegister, receptionistRegister.Image, "Receptionist");

            if (error != null)
            {
                ModelState.AddModelError("Register", error);
                return BadRequest(ModelState);
            }

            var receptionist = new Receptionist
            {
                UserId = user.Id,
                ShiftStart = receptionistRegister.ShiftStart,
                ShiftEnd = receptionistRegister.ShiftEnd
            };

            await receptionistRepository.AddReceptionist(receptionist);

            return Ok("Receptionist Registered Successfully");
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLogin UserFromRequest)
        {
            if (UserFromRequest == null)
            {
                return BadRequest(new
                {
                    Message = "User login data is null.",
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new 
                {
                    Message = "Validation failed.",
                });
            }

            ApplicationUser userFromDb = await _userManager.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == UserFromRequest.Email);

            if (userFromDb == null || !await _userManager.CheckPasswordAsync(userFromDb, UserFromRequest.Password))
            {
                return Unauthorized(new 
                {
                    Message = "Login failed.",
                    Errors = new List<string> { "Invalid username or password." }
                });
            }

            var userClaims = await _userManager.GetClaimsAsync(userFromDb);
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userFromDb.Id),
                new Claim(ClaimTypes.Name, userFromDb.UserName),
            };

            var roles = await _userManager.GetRolesAsync(userFromDb);
            foreach (var role in roles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }


            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]));
            var credentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(30);
            var jwtToken = new JwtSecurityToken(
                audience: _config["JWT:AudienceIP"],
                issuer: _config["JWT:IssuerIP"],
                claims: authClaims,
                expires: accessTokenExpiration,
                signingCredentials: credentials
            );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                CreatedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                UserId = userFromDb.Id
            };

            userFromDb.RefreshTokens.Add(refreshToken);
            await _userManager.UpdateAsync(userFromDb);

            return Ok(new
            {
                token = accessToken,
                expiration = accessTokenExpiration,
                refreshToken = refreshToken.Token,
                refreshTokenExpiry = refreshToken.ExpiryDate
            });
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody] AuthResultDTO request)
        {
            try
            {
                //Refresh token request received
                if (string.IsNullOrEmpty(request?.AccessToken) || string.IsNullOrEmpty(request?.RefreshToken))
                    return BadRequest("Access token and refresh token are required.");

                var principal = _authService.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                    return BadRequest("Invalid access token.");

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.Users
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return Unauthorized("User not found.");

                var storedRefreshToken = user.RefreshTokens?.FirstOrDefault(rt => rt.Token == request.RefreshToken);
                if (storedRefreshToken == null)
                    return Unauthorized("Invalid refresh token.");

                if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                    return Unauthorized("Expired refresh token.");

                var result = await _authService.GenerateTokenAsync(user, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] string Email){
            if (string.IsNullOrEmpty(Email))
                return BadRequest("Invalid Payload");

            var user = await _userManager.FindByEmailAsync(Email);
            if (user == null)
                return BadRequest("Invalid Payload");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            if(token == null){
                return BadRequest("Somwthing Went Wrong");
            }

            var callBackUrl = $"https://localhost:7242/ResetPassword?token={token}&email={Email}";

            // send email 
            var request = new MailRequestDTO
            {
                ToEmail = Email,
                Subject = "Clinic-System | Reset Password",
                Body = callBackUrl
            };

            if (string.IsNullOrWhiteSpace(request.ToEmail) || string.IsNullOrWhiteSpace(request.Subject) || string.IsNullOrWhiteSpace(request.Body))
                return BadRequest("Email, subject, and body are required.");

            await _mailingServices.SendEmailAsync(request.ToEmail, request.Subject, request.Body, request.Attachments);

            return Ok("Email sent successfully.");
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if(!ModelState.IsValid)
                return BadRequest("Invalid Payload");

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return BadRequest("Invalid Payload");

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
                return BadRequest("Something Wrong");

            return Ok("Password Reset Successfully");
        }
    }
}