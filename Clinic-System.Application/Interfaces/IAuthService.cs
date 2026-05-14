using Clinic_System.Application.DTO;
using Clinic_System.Domain.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Clinic_System.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResultDTO> GenerateTokenAsync(ApplicationUser user, string ipAddress, HttpResponse response, IList<string> roles);
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<RefreshToken> GetSavedRefreshTokenAsync(string userName , string refreshToken);
        Task RevokeRefreshToken(RefreshToken refreshToken);
        List<Claim> GetClaims(ApplicationUser user);    
    }
}
