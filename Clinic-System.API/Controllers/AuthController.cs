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
using System.ComponentModel.DataAnnotations;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly PatientRepository _patientRepository;
        private readonly ReceptionistRepository _receptionistRepository;
        private readonly IRegisterService _registerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMailingServices _mailingServices;

        public AuthController(
            PatientRepository patientRepository,
            ReceptionistRepository receptionistRepository,
            IRegisterService registerService,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IAuthService authService,
            ILogger<AuthController> logger,
            IMailingServices mailingServices,
            IDoctorService doctorService)
        {
            _patientRepository = patientRepository ?? throw new ArgumentNullException(nameof(patientRepository));
            _receptionistRepository = receptionistRepository ?? throw new ArgumentNullException(nameof(receptionistRepository));
            _registerService = registerService ?? throw new ArgumentNullException(nameof(registerService));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mailingServices = mailingServices ?? throw new ArgumentNullException(nameof(mailingServices));
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
        }

        [HttpPost("DoctorRegister")]
        public async Task<IActionResult> DoctorRegister([FromForm][Required] DoctorRegister doctorRegister)
        {
            try
            {
                if (doctorRegister == null)
                    return BadRequest(new { Message = "Doctor registration data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid data", Errors = ModelState });

                // Additional business validation
                if (doctorRegister.SpecialityId <= 0)
                    return BadRequest(new { Message = "Valid speciality is required" });

                var (error, user) = await _registerService.RegisterUserAsync(doctorRegister, doctorRegister.Image, "Doctor");

                if (error != null)
                {
                    _logger.LogWarning("Doctor registration failed: {Error}", error);
                    return BadRequest(new { Message = "Registration failed", Error = error });
                }

                var doctor = new Doctor
                {
                    UserId = user.Id,
                    SpecialityId = doctorRegister.SpecialityId
                };

                await _doctorService.AddDoctor(doctor);

                _logger.LogInformation("Doctor registered successfully with ID: {UserId}", user.Id);
                return CreatedAtAction(nameof(DoctorRegister), new { Message = "Doctor registered successfully", UserId = user.Id });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument during doctor registration");
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error during doctor registration");
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during doctor registration");
                return StatusCode(500, new { Message = "An error occurred during registration" });
            }
        }

        [HttpPost("PatientRegister")]
        public async Task<IActionResult> PatientRegister([FromForm][Required] PatientRegister patientRegister)
        {
            try
            {
                if (patientRegister == null)
                    return BadRequest(new { Message = "Patient registration data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid data", Errors = ModelState });

                var (error, user) = await _registerService.RegisterUserAsync(patientRegister, patientRegister.Image, "Patient");

                if (error != null)
                {
                    _logger.LogWarning("Patient registration failed: {Error}", error);
                    return BadRequest(new { Message = "Registration failed", Error = error });
                }

                var patient = new Patient
                {
                    UserId = user.Id,
                    BloodType = patientRegister.BloodType,
                    MedicalHistory = patientRegister.MedicalHistory
                };

                await _patientRepository.AddPatient(patient);

                _logger.LogInformation("Patient registered successfully with ID: {UserId}", user.Id);
                return CreatedAtAction(nameof(PatientRegister), new { Message = "Patient registered successfully", UserId = user.Id });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument during patient registration");
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error during patient registration");
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during patient registration");
                return StatusCode(500, new { Message = "An error occurred during registration" });
            }
        }

        [HttpPost("ReceptionRegister")]
        public async Task<IActionResult> ReceptionistRegister([FromForm][Required] ReceptionistRegister receptionistRegister)
        {
            try
            {
                if (receptionistRegister == null)
                    return BadRequest(new { Message = "Receptionist registration data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid data", Errors = ModelState });

                // Additional business validation
                if (receptionistRegister.ShiftStart >= receptionistRegister.ShiftEnd)
                    return BadRequest(new { Message = "Shift start time must be before shift end time" });

                var (error, user) = await _registerService.RegisterUserAsync(receptionistRegister, receptionistRegister.Image, "Receptionist");

                if (error != null)
                {
                    _logger.LogWarning("Receptionist registration failed: {Error}", error);
                    return BadRequest(new { Message = "Registration failed", Error = error });
                }

                var receptionist = new Receptionist
                {
                    UserId = user.Id,
                    ShiftStart = receptionistRegister.ShiftStart,
                    ShiftEnd = receptionistRegister.ShiftEnd
                };

                await _receptionistRepository.AddReceptionist(receptionist);

                _logger.LogInformation("Receptionist registered successfully with ID: {UserId}", user.Id);
                return CreatedAtAction(nameof(ReceptionistRegister), new { Message = "Receptionist registered successfully", UserId = user.Id });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid argument during receptionist registration");
                return BadRequest(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation error during receptionist registration");
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during receptionist registration");
                return StatusCode(500, new { Message = "An error occurred during registration" });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody][Required] UserLogin userFromRequest)
        {
            try
            {
                if (userFromRequest == null)
                {
                    return BadRequest(new { Message = "Login credentials are required" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(new { Message = "Invalid login data", Errors = ModelState });
                }

                // Additional validation
                if (string.IsNullOrWhiteSpace(userFromRequest.Email) || string.IsNullOrWhiteSpace(userFromRequest.Password))
                {
                    return BadRequest(new { Message = "Email and password are required" });
                }

                var userFromDb = await _userManager.Users
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Email == userFromRequest.Email);

                if (userFromDb == null)
                {
                    _logger.LogWarning("Login attempt with non-existent email: {Email}", userFromRequest.Email);
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                if (!await _userManager.CheckPasswordAsync(userFromDb, userFromRequest.Password))
                {
                    _logger.LogWarning("Failed login attempt for user: {Email}", userFromRequest.Email);
                    return Unauthorized(new { Message = "Invalid credentials" });
                }

                // Check if email is confirmed (if email confirmation is required)
                if (!userFromDb.EmailConfirmed && _userManager.Options.SignIn.RequireConfirmedEmail)
                {
                    return Unauthorized(new { Message = "Email not confirmed" });
                }

                // Check if account is locked
                if (await _userManager.IsLockedOutAsync(userFromDb))
                {
                    _logger.LogWarning("Login attempt on locked account: {Email}", userFromRequest.Email);
                    return Unauthorized(new { Message = "Account is locked" });
                }

                var userClaims = await _userManager.GetClaimsAsync(userFromDb);
                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userFromDb.Id),
                    new Claim(ClaimTypes.Name, userFromDb.UserName ?? string.Empty),
                    new Claim(ClaimTypes.Email, userFromDb.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                authClaims.AddRange(userClaims);

                var roles = await _userManager.GetRolesAsync(userFromDb);
                authClaims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var jwtSettings = _config.GetSection("JWT");
                var secretKey = jwtSettings["SecretKey"];

                if (string.IsNullOrEmpty(secretKey))
                {
                    _logger.LogError("JWT SecretKey not configured");
                    return StatusCode(500, new { Message = "Server configuration error" });
                }

                var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
                var credentials = new SigningCredentials(signInKey, SecurityAlgorithms.HmacSha256);

                var accessTokenExpiration = DateTime.UtcNow.AddMinutes(30);
                var jwtToken = new JwtSecurityToken(
                    audience: jwtSettings["AudienceIP"],
                    issuer: jwtSettings["IssuerIP"],
                    claims: authClaims,
                    expires: accessTokenExpiration,
                    signingCredentials: credentials
                );

                var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                // Clean up old refresh tokens
                var expiredTokens = userFromDb.RefreshTokens?.Where(rt => rt.ExpiryDate < DateTime.UtcNow).ToList();
                if (expiredTokens?.Any() == true)
                {
                    foreach (var token in expiredTokens)
                    {
                        userFromDb.RefreshTokens.Remove(token);
                    }
                }

                var refreshToken = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString(),
                    CreatedDate = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    UserId = userFromDb.Id
                };

                userFromDb.RefreshTokens ??= new List<RefreshToken>();
                userFromDb.RefreshTokens.Add(refreshToken);

                await _userManager.UpdateAsync(userFromDb);

                _logger.LogInformation("User logged in successfully: {Email}", userFromRequest.Email);

                return Ok(new
                {
                    token = accessToken,
                    expiration = accessTokenExpiration,
                    refreshToken = refreshToken.Token,
                    refreshTokenExpiry = refreshToken.ExpiryDate,
                    user = new
                    {
                        id = userFromDb.Id,
                        email = userFromDb.Email,
                        userName = userFromDb.UserName,
                        roles = roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user: {Email}", userFromRequest?.Email);
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh([FromBody][Required] AuthResultDTO request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { Message = "Token refresh data is required" });

                if (string.IsNullOrWhiteSpace(request.AccessToken) || string.IsNullOrWhiteSpace(request.RefreshToken))
                    return BadRequest(new { Message = "Access token and refresh token are required" });

                var principal = _authService.GetPrincipalFromExpiredToken(request.AccessToken);
                if (principal == null)
                {
                    _logger.LogWarning("Invalid access token provided for refresh");
                    return BadRequest(new { Message = "Invalid access token" });
                }

                var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return BadRequest(new { Message = "Invalid token claims" });

                var user = await _userManager.Users
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    _logger.LogWarning("Token refresh attempted for non-existent user: {UserId}", userId);
                    return Unauthorized(new { Message = "User not found" });
                }

                var storedRefreshToken = user.RefreshTokens?.FirstOrDefault(rt => rt.Token == request.RefreshToken);
                if (storedRefreshToken == null)
                {
                    _logger.LogWarning("Invalid refresh token used: {UserId}", userId);
                    return Unauthorized(new { Message = "Invalid refresh token" });
                }

                if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired refresh token used: {UserId}", userId);
                    // Clean up expired token
                    user.RefreshTokens.Remove(storedRefreshToken);
                    await _userManager.UpdateAsync(user);
                    return Unauthorized(new { Message = "Refresh token expired" });
                }

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result = await _authService.GenerateTokenAsync(user, clientIp);

                // Remove old refresh token and save the new one
                user.RefreshTokens.Remove(storedRefreshToken);
                await _userManager.UpdateAsync(user);

                _logger.LogInformation("Token refreshed successfully for user: {UserId}", userId);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Security token exception during refresh");
                return BadRequest(new { Message = "Invalid token" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new { Message = "An error occurred while refreshing the token" });
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody][Required][EmailAddress] string email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { Message = "Email address is required" });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid email format", Errors = ModelState });

                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    // Don't reveal that the user doesn't exist for security reasons
                    _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                    return Ok(new { Message = "If the email exists, a reset link has been sent" });
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to generate password reset token for user: {Email}", email);
                    return StatusCode(500, new { Message = "Failed to generate reset token" });
                }

                // URL encode the token and email for safety
                var encodedToken = Uri.EscapeDataString(token);
                var encodedEmail = Uri.EscapeDataString(email);
                var callBackUrl = $"https://localhost:7242/ResetPassword?token={encodedToken}&email={encodedEmail}";

                var mailRequest = new MailRequestDTO
                {
                    ToEmail = email,
                    Subject = "Clinic-System | Password Reset Request",
                    Body = $"Please click the following link to reset your password: {callBackUrl}",
                    Attachments = null
                };

                await _mailingServices.SendEmailAsync(mailRequest.ToEmail, mailRequest.Subject, mailRequest.Body, mailRequest.Attachments);

                _logger.LogInformation("Password reset email sent to: {Email}", email);
                return Ok(new { Message = "Password reset email sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset email to: {Email}", email);
                return StatusCode(500, new { Message = "An error occurred while processing the request" });
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody][Required] ResetPasswordRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { Message = "Reset password data is required" });

                if (!ModelState.IsValid)
                    return BadRequest(new { Message = "Invalid reset data", Errors = ModelState });

                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Token) ||
                    string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { Message = "Email, token, and new password are required" });
                }

                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Password reset attempted for non-existent email: {Email}", request.Email);
                    return BadRequest(new { Message = "Invalid reset request" });
                }

                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
                if (!result.Succeeded)
                {
                    _logger.LogWarning("Password reset failed for user: {Email}. Errors: {Errors}",
                        request.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return BadRequest(new { Message = "Password reset failed", Errors = errors });
                }

                _logger.LogInformation("Password reset successfully for user: {Email}", request.Email);
                return Ok(new { Message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for: {Email}", request?.Email);
                return StatusCode(500, new { Message = "An error occurred while resetting the password" });
            }
        }
    }
}