using Expence.Application.Interface;
using Expence.Domain.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Expence.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration; 
        }
        public string GenerateRefreshToken(User user)
        {
            throw new NotImplementedException();
        }

        public string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));     
        }

        public ClaimsPrincipal GetClaimsPrincipalFromExpiredToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
