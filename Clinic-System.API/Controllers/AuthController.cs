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
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Newtonsoft.Json.Linq;
using Sprache;
using Clinic_System.Domain.Constant;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly PatientRepository _patientRepository;
        private readonly ReceptionistRepository _receptionistRepository;
        private readonly IRegisterService _registerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMailingServices _mailingServices;
        private readonly AppDbContext _context;

        public AuthController(
            PatientRepository patientRepository,
            ReceptionistRepository receptionistRepository,
            IRegisterService registerService,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IAuthService authService,
            ILogger<AuthController> logger,
            IMailingServices mailingServices,
            IDoctorService doctorService,
            IPatientService patientService,
            AppDbContext context)
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
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _context = context;
        }

        [HttpPost("DoctorRegister")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DoctorRegister([FromForm][Required] DoctorRegisterDTO doctorRegister)
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

                var body = $@"
                        <!DOCTYPE html>
                        <html>
                          <body>
                            <h2>Welcome to Clinic-System 👩‍⚕️</h2>
                            <p>Dear Doctor,</p>
                            <p>Your account has been successfully created.</p>
                            <p><b>Email:</b> {doctorRegister.Email}</p>
                            <p><b>Password:</b> {doctorRegister.Password}</p>
                            <p>Please change your password after first login.</p>
                          </body>
                        </html>";

                var mailRequest = new MailRequestDTO
                {
                    ToEmail = doctorRegister.Email,
                    Subject = "Clinic-System | Your Account Credentials",
                    Body = body,
                    Attachments = null
                };

                await _mailingServices.SendEmailAsync(
                    mailRequest.ToEmail,
                    mailRequest.Subject,
                    mailRequest.Body,
                    mailRequest.Attachments
                );


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
        public async Task<IActionResult> PatientRegister([FromForm][Required] PatientRegisterDTO patientRegister)
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
        public async Task<IActionResult> ReceptionistRegister([FromForm][Required] ReceptionistRegisterDTO receptionistRegister)
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

                var body = $@"
                        <!DOCTYPE html>
                        <html>
                          <body>
                            <h2>Welcome to Clinic-System 👩‍⚕️</h2>
                            <p>Dear Receptionist,</p>
                            <p>Your account has been successfully created.</p>
                            <p><b>Email:</b> {receptionistRegister.Email}</p>
                            <p><b>Password:</b> {receptionistRegister.Password}</p>
                            <p>Please change your password after first login.</p>
                          </body>
                        </html>";

                var mailRequest = new MailRequestDTO
                {
                    ToEmail = receptionistRegister.Email,
                    Subject = "Clinic-System | Your Account Credentials",
                    Body = body,
                    Attachments = null
                };

                await _mailingServices.SendEmailAsync(
                    mailRequest.ToEmail,
                    mailRequest.Subject,
                    mailRequest.Body,
                    mailRequest.Attachments
                );

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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                // Validate request
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { Message = "Email and password are required" });
                }

                _logger.LogInformation("Login attempt for email: {Email}", request.Email);

                // Find user by email
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found for email: {Email}", request.Email);
                    return Unauthorized(new { Message = "Invalid email or password" });
                }

                // Check password
                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    _logger.LogWarning("Login failed: Invalid password for email: {Email}", request.Email);
                    return Unauthorized(new { Message = "Invalid email or password" });
                }

                // Generate tokens
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result = await _authService.GenerateTokenAsync(user, clientIp);

                if (result == null)
                {
                    _logger.LogError("Failed to generate tokens for user: {Email}", request.Email);
                    return StatusCode(500, new { Message = "Failed to generate authentication tokens" });
                }

                // Parse JWT to get expiration
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(result.AccessToken);

                // Set cookies using the result object
                Response.Cookies.Append("t", result.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = jwtToken.ValidTo
                });

                Response.Cookies.Append("rt", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                // Add CORS headers
                Response.Headers.Add("Access-Control-Allow-Credentials", "true");

                _logger.LogInformation("Login successful for user: {Email}", request.Email);
                _logger.LogInformation("Access token expires at: {ExpiryTime}", jwtToken.ValidTo);

                return Ok(new
                {
                    Message = "Login successful",
                    ExpiresAt = jwtToken.ValidTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login");
                return StatusCode(500, new { Message = "An error occurred during login" });
            }
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                _logger.LogInformation("Token refresh attempt started");

                // Debug: Log all cookies received
                _logger.LogInformation($"Total cookies received: {Request.Cookies.Count}");
                foreach (var cookie in Request.Cookies)
                {
                    _logger.LogInformation($"Cookie: {cookie.Key} = {cookie.Value?.Substring(0, Math.Min(20, cookie.Value.Length))}...");
                }

                // Read refresh token from cookies
                if (!Request.Cookies.TryGetValue("rt", out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
                {
                    _logger.LogWarning("Token refresh failed: Missing refresh token cookie");
                    return BadRequest(new { Message = "Refresh token cookie is required" });
                }

                // Try to get access token, but it might be expired/missing
                Request.Cookies.TryGetValue("t", out var accessToken);

                string userId = null;

                // Try to extract user info from access token if available
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    try
                    {
                        var principal = _authService.GetPrincipalFromExpiredToken(accessToken);
                        userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        _logger.LogInformation("Extracted user ID from access token: {UserId}", userId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation("Could not extract user from access token (this is normal): {Error}", ex.Message);
                    }
                }

                // If we couldn't get userId from token, find user by refresh token
                ApplicationUser user;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get user by ID from token
                    user = await _userManager.Users
                        .Include(u => u.RefreshTokens)
                        .FirstOrDefaultAsync(u => u.Id == userId);
                }
                else
                {
                    // Find user by refresh token directly
                    _logger.LogInformation("No valid access token, finding user by refresh token");

                    // Find the refresh token in database first
                    var refreshTokenEntity = await _context.RefreshTokens
                        .Include(rt => rt.User)
                        .ThenInclude(u => u.RefreshTokens)
                        .FirstOrDefaultAsync(rt => rt.Token == refreshToken);

                    if (refreshTokenEntity == null)
                    {
                        _logger.LogWarning("Refresh token not found in database");
                        return Unauthorized(new { Message = "Invalid refresh token" });
                    }

                    user = refreshTokenEntity.User;
                    userId = user.Id;
                    _logger.LogInformation("Found user by refresh token: {UserId}", userId);
                }

                if (user == null)
                {
                    _logger.LogWarning("User not found for refresh token");
                    return Unauthorized(new { Message = "User not found" });
                }

                // Validate refresh token
                var storedRefreshToken = user.RefreshTokens?.FirstOrDefault(rt => rt.Token == refreshToken);
                if (storedRefreshToken == null)
                {
                    _logger.LogWarning("Refresh token not found in user's tokens for user: {UserId}", userId);
                    return Unauthorized(new { Message = "Invalid refresh token" });
                }

                if (storedRefreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired for user {UserId}. Expired at: {ExpiryDate}",
                        userId, storedRefreshToken.ExpiryDate);

                    // Clean up expired token
                    storedRefreshToken.ExpiryDate = DateTime.UtcNow.AddDays(7);
                    await _userManager.UpdateAsync(user);
                    return Unauthorized(new { Message = "Refresh token expired" });
                }

                _logger.LogInformation("Refresh token validated successfully, generating new tokens");

                // Generate new tokens
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result = await _authService.GenerateTokenAsync(user, clientIp);

                if (result == null)
                {
                    _logger.LogError("Failed to generate new tokens for user: {UserId}", userId);
                    return StatusCode(500, new { Message = "Failed to generate new tokens" });
                }

                // Remove old refresh token from database
                user.RefreshTokens.Remove(storedRefreshToken);
                await _userManager.UpdateAsync(user);

                // Get new token expiration info
                var jwtHandler = new JwtSecurityTokenHandler();
                var jwtToken = jwtHandler.ReadJwtToken(result.AccessToken);

                // Set new cookies
                Response.Cookies.Append("t", result.AccessToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = jwtToken.ValidTo,
                });

                Response.Cookies.Append("rt", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(7),
                });

                Response.Headers.Add("Access-Control-Allow-Credentials", "true");

                _logger.LogInformation("Token refresh completed successfully for user: {UserId}", userId);
                _logger.LogInformation("New access token expires at: {ExpiryTime}", jwtToken.ValidTo);

                return Ok(new
                {
                    Message = "Token refreshed successfully",
                    ExpiresAt = jwtToken.ValidTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during token refresh");
                return StatusCode(500, new { Message = "An error occurred while refreshing the token" });
            }
        }
        [HttpGet("Me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var id = "";
                if(role == "Doctor")
                {
                    var doctor = await _doctorService.GetDoctorByUserIdAsync(userId);
                    id = doctor.Id;
                }else if(role == "Receptionist")
                {
                    var receptionist = await _receptionistRepository.GetReceptionistByUserIdAsync(userId);
                    id = receptionist.Id.ToString();
                }else if(role == "Patient")
                {
                    var patient = await _patientService.GetPatientByUserIdAsync(userId);
                    id = patient.Id.ToString();
                }
                else
                {
                    id = userId;
                }
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                Console.WriteLine($"JWT Authentication Status: {User.Identity.IsAuthenticated}");
                Console.WriteLine($"UserId: {userId}, Role: {role}, Email: {email}");

                if (User.Identity.IsAuthenticated && userId != null && role != null)
                {
                    return Ok(new
                    {
                        Message = "User Retrieved Successfully",
                        user = new { userId, id, role, email },
                        isAuthenticated = true
                    });
                }
                else
                {
                    return Ok(new
                    {
                        Message = "User UN Authorized",
                        user = new { userId, role },
                        isAuthenticated = false
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCurrentUser: {ex.Message}");
                return StatusCode(500, new { Message = "Internal server error", isAuthenticated = false });
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
                var frontendUrl = _config["Frontend:BaseUrl"];
                var callBackUrl = $"{frontendUrl}/reset-password?token={encodedToken}&email={encodedEmail}";


                var body = $@"
                            <!DOCTYPE html>
                            <html>
                              <body style='font-family: Arial, sans-serif; line-height:1.6; color:#333;'>
                                <h2 style='color:#1976d2;'>Clinic-System Password Reset</h2>
                                <p>Dear user,</p>
                                <p>We received a request to reset your password. Please click the button below to set a new password:</p>
                                <p>
                                  <a href='{callBackUrl}' 
                                     style='display:inline-block; padding:10px 20px; background:#1976d2; color:white; text-decoration:none; border-radius:5px;'>
                                    Reset Password
                                  </a>
                                </p>
                                <p>If you did not request a password reset, you can safely ignore this email.</p>
                                <br />
                                <p style='font-size:12px; color:#666;'>Clinic-System Team</p>
                              </body>
                            </html>";

                var mailRequest = new MailRequestDTO
                {
                    ToEmail = email,
                    Subject = "Clinic-System | Password Reset Request",
                    Body = body,
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

        [HttpGet("Logout")]
        public IActionResult Logout()
        {
            try
            {
                // Clear the cookies
                Response.Cookies.Delete("t", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });
                Response.Cookies.Delete("rt", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });

                return Ok(new { Message = "Logout successful" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { Message = "An error occurred during logout" });
            }
        }
    }
}