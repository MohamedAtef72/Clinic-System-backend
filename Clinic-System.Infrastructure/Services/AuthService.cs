using Clinic_System.Application.DTO;
using Clinic_System.Application.Interfaces;
using Clinic_System.Domain.Models;
using Clinic_System.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Clinic_System.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration config, AppDbContext context, ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _config = config;
            _context = context;
            _logger = logger;
        }

        public async Task<AuthResultDTO> GenerateTokenAsync(ApplicationUser user, string ipAddress, HttpResponse response, IList<string> roles)
        {
            if (user.IsDeleted)
                throw new InvalidOperationException("User account is deactivated.");

            var claims = GetClaims(user);
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var accessToken = GenerateAccessToken(claims);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenDaysConfig = _config["JWT:RefreshTokenExpirationDays"];
            var expirationDays = !string.IsNullOrEmpty(refreshTokenDaysConfig)
                ? int.Parse(refreshTokenDaysConfig)
                : 7;

            var expirationMinutes = GetAccessTokenExpirationMinutes();
            var hashedToken = HashToken(refreshToken);
            var utcNow = DateTime.UtcNow;
            var expiryDate = utcNow.AddDays(expirationDays);

            // Single round trip: MERGE does insert-or-update atomically
            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                MERGE RefreshTokens AS target
                USING (SELECT {user.Id} AS UserId) AS source
                ON target.UserId = source.UserId
                WHEN MATCHED THEN
                    UPDATE SET Token = {hashedToken},
                               ExpiryDate = {expiryDate},
                               CreatedDate = {utcNow},
                               CreatedByIp = {ipAddress},
                               IsRevoked = 0
                WHEN NOT MATCHED THEN
                    INSERT (UserId, Token, ExpiryDate, CreatedDate, CreatedByIp, IsRevoked)
                    VALUES ({user.Id}, {hashedToken}, {expiryDate}, {utcNow}, {ipAddress}, 0);
            ");

            // Cookies (unchanged)
            var isHttps = true;
            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = utcNow.AddMinutes(expirationMinutes + 3),
            };
            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = expiryDate
            };

            response.Cookies.Append("t", accessToken, accessOptions);
            response.Cookies.Append("rt", refreshToken, refreshOptions);

            return new AuthResultDTO { AccessToken = accessToken, RefreshToken = refreshToken };
        }
        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var jwtSettings = _config.GetSection("JWT");
            var expirationMinutes = GetAccessTokenExpirationMinutes();

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = creds
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(tokenDescriptor);

            return handler.WriteToken(token);
        }
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            _logger.LogInformation("[AuthService.GetPrincipalFromExpiredToken] Validating expired token");

            var jwtSettings = _config.GetSection("JWT");

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],

                ValidateAudience = false,
                //ValidAudience = jwtSettings["Audience"],

                ValidateLifetime = false, 

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])
                ),

                ClockSkew = TimeSpan.Zero
            };

            var handler = new JwtSecurityTokenHandler();

            try
            {
                var principal = handler.ValidateToken(token, tokenValidationParameters, out _);
                _logger.LogInformation("[AuthService.GetPrincipalFromExpiredToken] Token validated successfully");
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[AuthService.GetPrincipalFromExpiredToken] Token validation failed");
                return null;
            }
        }
        public List<Claim> GetClaims(ApplicationUser user)
        {
            return new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };
        }
        private double GetAccessTokenExpirationMinutes()
        {
            var expirationMinutes = _config["JWT:AccessTokenExpirationMinutes"];
            return !string.IsNullOrEmpty(expirationMinutes) ? double.Parse(expirationMinutes) : 30.0;
        }
        public async Task<RefreshToken> GetSavedRefreshTokenAsync(string userName, string refreshToken)
        {
            _logger.LogInformation("[AuthService.GetSavedRefreshTokenAsync] Retrieving refresh token for user: {UserName}", userName);

            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                _logger.LogWarning("[AuthService.GetSavedRefreshTokenAsync] User not found: {UserName}", userName);
                return null;
            }

            var hashedToken = HashToken(refreshToken);

            var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == user.Id && x.Token == hashedToken);

            if (savedToken == null)
            {
                _logger.LogWarning("[AuthService.GetSavedRefreshTokenAsync] Refresh token not found in database");
            }
            else
            {
                _logger.LogInformation("[AuthService.GetSavedRefreshTokenAsync] Refresh token found. IsRevoked: {IsRevoked}, ExpiryDate: {ExpiryDate}", 
                    savedToken.IsRevoked, savedToken.ExpiryDate);
            }

            return savedToken;
        }
        public async Task RevokeRefreshToken(RefreshToken refreshToken)
        {
            _logger.LogInformation("[AuthService.RevokeRefreshToken] Revoking refresh token");

            if (refreshToken != null)
            {
                // Mark the token as revoked instead of deleting it
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
                _logger.LogInformation("[AuthService.RevokeRefreshToken] Refresh token revoked successfully");
            }
            else
            {
                _logger.LogWarning("[AuthService.RevokeRefreshToken] Refresh token is null");
            }
        }
        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        private async Task SaveRefreshTokenAsync(string userId, string hashedToken, DateTime expiryDate, DateTime createdDate, string ip)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync($@"
                    MERGE RefreshTokens AS target
                    USING (SELECT {userId} AS UserId) AS source
                    ON target.UserId = source.UserId
                    WHEN MATCHED THEN
                        UPDATE SET Token = {hashedToken}, ExpiryDate = {expiryDate}, CreatedDate = {createdDate}, CreatedByIp = {ip}, IsRevoked = 0
                    WHEN NOT MATCHED THEN
                        INSERT (UserId, Token, ExpiryDate, CreatedDate, CreatedByIp, IsRevoked)
                        VALUES ({userId}, {hashedToken}, {expiryDate}, {createdDate}, {ip}, 0);
                ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist refresh token for user {UserId}", userId);
            }
        }
    }
}