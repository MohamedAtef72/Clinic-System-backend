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

        public async Task<AuthResultDTO> GenerateTokenAsync(ApplicationUser user, string ipAddress, HttpResponse response)
        {
            if (user.IsDeleted)
            {
                throw new InvalidOperationException("User account is deactivated.");
            }

            var roles = await _userManager.GetRolesAsync(user); 

            var claims = GetClaims(user); 

            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r))); 

            // 2. Generate Tokens
            var accessToken = GenerateAccessToken(claims); 

            var refreshToken = GenerateRefreshToken(); 

            // 3. Revoke old refresh tokens
            var oldTokens = await _context.RefreshTokens
                .AsNoTracking()
                .Where(x => x.UserId == user.Id && !x.IsRevoked)
                .ToListAsync(); 

            foreach (var token in oldTokens)
            {
                token.IsRevoked = true;
            }

            // 4. Save new refresh token
            var refreshTokenDaysConfig = _config["JWT:RefreshTokenExpirationDays"];
            var expirationDays = !string.IsNullOrEmpty(refreshTokenDaysConfig)
                ? int.Parse(refreshTokenDaysConfig)
                : 7;

            var expirationMinutes = GetAccessTokenExpirationMinutes();

            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(expirationDays),
                CreatedDate = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            await SaveRefreshToken(user, refreshToken); 

            // 5. Parse Access Token Expiry
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);


            // 6. Set Cookies
            var isHttps = true; // change to true if your app is served over HTTPS

            var accessOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes + 3),
            };

            var refreshOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = isHttps,
                SameSite = isHttps ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(expirationDays)
            };

            response.Cookies.Append("t", accessToken, accessOptions);

            response.Cookies.Append("rt", refreshToken, refreshOptions);

            // 7. Return DTO
            return new AuthResultDTO
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
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

        public async Task SaveRefreshToken(ApplicationUser user, string refreshToken)
        {
            var expirationDays = int.Parse(
                _config["JWT:RefreshTokenExpirationDays"] ?? "7");

            var expiry = DateTime.UtcNow.AddDays(expirationDays);

            var hashedToken = HashToken(refreshToken);

            var tokenEntity = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (tokenEntity == null)
            {
                tokenEntity = new RefreshToken
                {
                    UserId = user.Id
                };

                await _context.RefreshTokens.AddAsync(tokenEntity);
            }

            tokenEntity.Token = hashedToken;
            tokenEntity.ExpiryDate = expiry;
            tokenEntity.CreatedDate = DateTime.UtcNow;
            tokenEntity.IsRevoked = false;

            await _context.SaveChangesAsync();
        }
        private string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}