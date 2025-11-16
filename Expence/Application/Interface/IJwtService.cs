using Expence.Domain.Models;
using System.Security.Claims;

namespace Expence.Application.Interface
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        string GenerateRefreshToken(User user);
        ClaimsPrincipal GetClaimsPrincipalFromExpiredToken(string token);
    }
}
