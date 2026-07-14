using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Constant;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Clinic_System.Infrastructure.Services;
using CloudinaryDotNet.Actions;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Clinic_System.Domain.Constant.AppConstants;

namespace Clinic_System.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly IPatientService _patientService;
        private readonly IReceptionistService _receptionistService;
        private readonly IRegisterService _registerService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IMailingServices _mailingServices;
        private readonly ICacheService _cache;
        private readonly IPhotoService _photoService;
        private readonly AppDbContext _context;


        public AuthController(
            IPatientService patientService,
            IReceptionistService receptionistService,
            IRegisterService registerService,
            UserManager<ApplicationUser> userManager,
            IConfiguration config,
            IAuthService authService,
            ILogger<AuthController> logger,
            IMailingServices mailingServices,
            IDoctorService doctorService,
            ICacheService cache,
            IPhotoService photoService,
            AppDbContext context)
        {
            _receptionistService = receptionistService ;
            _registerService = registerService ;
            _userManager = userManager;
            _cache = cache ;
            _context = context ;
            _config = config ;
            _authService = authService ;
            _logger = logger ;
            _mailingServices = mailingServices ;
            _doctorService = doctorService ;
            _patientService = patientService ;
            _photoService = photoService;
        }

        [HttpPost("DoctorRegister")]
        [Authorize(Roles = Domain.Constant.Role.Admin)]
        public async Task<IActionResult> DoctorRegister([FromBody]DoctorRegisterDTO doctorRegister)
        {
            try
            {

                if (doctorRegister == null)
                {
                    _logger.LogWarning("[DOCTOR_REGISTER] Null registration data received");
                    return BadRequest(new { Message = "Doctor registration data is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("[DOCTOR_REGISTER] Invalid model state");
                    return BadRequest(new { Message = "Invalid data", Errors = ModelState });
                }

                if (doctorRegister.SpecialityId <= 0)
                {
                    _logger.LogWarning("[DOCTOR_REGISTER] Invalid speciality ID: {SpecialityId}", doctorRegister.SpecialityId);
                    return BadRequest(new { Message = "Valid speciality is required" });
                }

                // Track: User Registration
                var (error, user) = await _registerService.RegisterUserAsync(
                    doctorRegister, "Doctor");

                if (error != null)
                {
                    _logger.LogWarning("[DOCTOR_REGISTER] Registration failed: {Error}", error);
                    return BadRequest(new { Message = "Registration failed", Error = error });
                }
                _logger.LogInformation("[DOCTOR_REGISTER] User created successfully. UserId: {UserId}", user?.Id);

                // Track: Doctor Creation/Restoration
                var doctor = await _doctorService.EnsureDoctorExistsOrRestoreAsync(
                    user.Id, doctorRegister.SpecialityId);


                await _cache.BumpVersionAsync("doctors:list");
                await _cache.BumpVersionAsync("admin:dashboard");

                // Track: Email Sending
                var body = $@"
                                <!DOCTYPE html>
                                <html>
                                  <body>
                                    <h2>Welcome to Clinic-System</h2>
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

                BackgroundJob.Enqueue(() =>
                    _mailingServices.SendEmailAsync(
                        mailRequest.ToEmail,
                        mailRequest.Subject,
                        mailRequest.Body,
                        mailRequest.Attachments
                    )
                );

                return Ok(new
                {
                    Message = "Doctor registered/restored successfully",
                    UserId = user.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DOCTOR_REGISTER] Unexpected error during doctor registration {ex.Message}");
                return StatusCode(500, new { Message = ex.Message, StackTrace = ex.StackTrace });
            }
        }
        [HttpPost("PatientRegister")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> PatientRegister([FromBody]PatientRegisterDTO patientRegister)
        {
            try
            {

                if (patientRegister == null)
                {
                    _logger.LogWarning("[PATIENT_REGISTER] Null registration data received");
                    return BadRequest(new { Message = "Patient registration data is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("[PATIENT_REGISTER] Invalid model state");
                    return BadRequest(new { Message = "Invalid data", Errors = ModelState });
                }

                // Track: User Registration
                var (error, user) = await _registerService.RegisterUserAsync(patientRegister, "Patient");

                if (error != null)
                {
                    _logger.LogWarning("[PATIENT_REGISTER] Registration failed: {Error}", error);
                    return BadRequest(new { Message = "Registration failed", Error = error });
                }

                // Track: Patient Creation/Restoration
                var patient = await _patientService.EnsurePatientExistsOrRestoreAsync(
                    user.Id, patientRegister.BloodType, patientRegister.MedicalHistory);

                // Track: Cache Invalidation
                await _cache.BumpVersionAsync("patients:list");

                return CreatedAtAction(nameof(PatientRegister), new { Message = "Patient registered successfully", UserId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[PATIENT_REGISTER] Unexpected error during patient registration {ex.Message}");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("ReceptionRegister")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> ReceptionistRegister([FromBody] ReceptionistRegisterDTO receptionistRegister)
        {
            try
            {

                if (receptionistRegister == null)
                {
                    _logger.LogWarning("[RECEPTIONIST_REGISTER] Null registration data received");
                    return BadRequest(new { Message = "Receptionist registration data is required" });
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("[RECEPTIONIST_REGISTER] Invalid model state");
                    return BadRequest(new { Message = "Invalid data", Errors = ModelState });
                }

                // Additional business validation
                if (receptionistRegister.ShiftStart >= receptionistRegister.ShiftEnd)
                {
                    _logger.LogWarning("[RECEPTIONIST_REGISTER] Invalid shift times - Start: {ShiftStart}, End: {ShiftEnd}", 
                        receptionistRegister.ShiftStart, receptionistRegister.ShiftEnd);
                    return BadRequest(new { Message = "Shift start time must be before shift end time" });
                }

                // Track: User Registration
                var (error, user) = await _registerService.RegisterUserAsync(receptionistRegister, "Receptionist");

                if (error != null)
                {
                    _logger.LogWarning("[RECEPTIONIST_REGISTER] Registration failed: {Error}", error);
                    return BadRequest(new { Message = "Registration failed", Error = error });
                }
                // Track: Receptionist Creation
                var receptionist = new Receptionist
                {
                    UserId = user.Id,
                    ShiftStart = receptionistRegister.ShiftStart,
                    ShiftEnd = receptionistRegister.ShiftEnd
                };

                await _receptionistService.AddReceptionist(receptionist);
                // Track: Email Sending
                var body = $@"
                        <!DOCTYPE html>
                        <html>
                          <body>
                            <h2>Welcome to Clinic-System </h2>
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

                BackgroundJob.Enqueue(() =>
                    _mailingServices.SendEmailAsync(
                    mailRequest.ToEmail,
                    mailRequest.Subject,
                    mailRequest.Body,
                    mailRequest.Attachments
                    )
                );

                return CreatedAtAction(nameof(ReceptionistRegister), new { Message = "Receptionist registered successfully", UserId = user.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError($"[RECEPTIONIST_REGISTER] Unexpected error during receptionist registration {ex.Message}");
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("Login")]
        [EnableRateLimiting("AuthPolicy")]  // brute-force protection — was blocked by GlobalLimiter before
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { Message = "Email and password are required" });
                }

                var normalizedEmail = request.Email.ToUpperInvariant();

                // Single round trip: user + roles in one query
                var loginData = await _context.Users
                    .Where(u => u.NormalizedEmail == normalizedEmail && !u.IsDeleted)
                    .Select(u => new
                    {
                        User = u,
                        Roles = (from ur in _context.UserRoles
                                 join r in _context.Roles on ur.RoleId equals r.Id
                                 where ur.UserId == u.Id
                                 select r.Name).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (loginData == null)
                {
                    return Unauthorized(new { Message = "Invalid email or password" });
                }

                var user = loginData.User;

                var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!validPassword)
                {
                    return Unauthorized(new { Message = "Invalid email or password" });
                }

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var result = await _authService.GenerateTokenAsync(user, clientIp, Response, loginData.Roles);

                sw.Stop();
                _logger.LogInformation("Elapsed time for Login endpoint: {ElapsedMs} ms", sw.Elapsed.TotalMilliseconds);

                return Ok(new { Message = "Login successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred during login", Error = ex.Message });
            }
        }

        [HttpPost("Refresh")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                // Log all cookies
                foreach (var cookie in Request.Cookies)
                {
                    var tokenPreview = cookie.Value?.Substring(0, Math.Min(20, cookie.Value.Length)) + "...";
                }

                // 1. Get tokens from cookies
                var accessToken = Request.Cookies["t"];
                var refreshToken = Request.Cookies["rt"];

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Unauthorized(new { Message = "Missing refresh token" });
                }

                if (string.IsNullOrEmpty(accessToken))
                {
                    return Unauthorized(new { Message = "Missing access token" });
                }

                // 2. Validate expired access token
                var principal = _authService.GetPrincipalFromExpiredToken(accessToken);

                if (principal == null)
                {
                    return Unauthorized(new { Message = "Invalid access token" });
                }

                // 3. Get user identity
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Invalid token claims" });
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.IsDeleted)
                {
                    return Unauthorized(new { Message = "User not found" });
                }

                // 4. Validate refresh token from DB
                var savedToken = await _authService.GetSavedRefreshTokenAsync(user.UserName, refreshToken);

                if (savedToken == null)
                {
                    return Unauthorized(new { Message = "Invalid refresh token" });
                }

                if (savedToken.IsRevoked)
                {
                    return Unauthorized(new { Message = "Refresh token is revoked" });
                }

                if (savedToken.ExpiryDate <= DateTime.UtcNow)
                {
                    return Unauthorized(new { Message = "Refresh token expired" });
                }

                // 5. IMPORTANT: revoke current refresh token
                await _authService.RevokeRefreshToken(savedToken);

                // 6. Generate new tokens — extract roles from the existing (expired) JWT claims
                //    so we avoid a GetRolesAsync DB call on every refresh.
                var roles = principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result = await _authService.GenerateTokenAsync(user, clientIp, Response, roles);

                return Ok(new
                {
                    Message = "Token refreshed successfully",
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Refresh failed",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("Me")]
        [EnableRateLimiting("ReadPolicy")]
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
                    var receptionist = await _receptionistService.GetReceptionistByUserIdAsync(userId);
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

                object response;
                if (User.Identity.IsAuthenticated && userId != null && role != null)
                {
                    response = new
                    {
                        Message = "User Retrieved Successfully",
                        user = new { userId, id, role, email },
                        isAuthenticated = true
                    };
                }
                else
                {
                    response = new
                    {
                        Message = "User UN Authorized",
                        user = new { userId, role },
                        isAuthenticated = false
                    };
                }
                //await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(1));
                return Ok(response);
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

        [HttpGet("GetUploadSignature")]
        [EnableRateLimiting("ReadPolicy")]
        public async Task<IActionResult> GetUploadSignature([FromQuery] string folder = "clinic_app_images")
        {
            try
            {
                _logger.LogInformation("[GET_UPLOAD_SIGNATURE] call method");

                var signature = await _photoService.GetUploadSignatureAsync(folder);
                _logger.LogInformation("[GET_UPLOAD_SIGNATURE] Signature is: {Signature}", signature);


                _logger.LogInformation("[GET_UPLOAD_SIGNATURE] Signature generated successfully");
                return Ok(new
                {
                    Message = "Upload signature generated successfully",
                    Data = signature
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GET_UPLOAD_SIGNATURE] Error generating upload signature");
                return StatusCode(500, new { Message = "Failed to generate upload signature", Error = ex.Message });
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