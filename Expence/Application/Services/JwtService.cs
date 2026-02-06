using Expence.Application.Interface;
using Expence.Domain.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Experimental;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Expence.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public RefreshToken GenerateRefreshToken(User user)
        {
            _logger.LogInformation("Generating refresh token for UserId: {UserId}", user.Id);

            //check meaning of this code
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Convert.ToBase64String(randomBytes),
                ExpiryDate = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogDebug("Refresh token generated successfully for UserId: {UserId}, Expiry: {ExpiryDate}",
                user.Id, refreshToken.ExpiryDate);

            return refreshToken;
        }

        public string GenerateToken(User user)
        {
            _logger.LogInformation("Generating JWT token for UserId: {UserId}, Email: {Email}", user.Id, user.Email);

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
            };

            var expiryTime = DateTime.Now.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer:_configuration["Jwt:Issuer"],
                audience:_configuration["Jwt:Audience"],
                claims,
                expires: expiryTime,
                signingCredentials: credentials
                );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogDebug("JWT token generated successfully for UserId: {UserId}, Expiry: {ExpiryTime}, TokenLength: {TokenLength}",
                user.Id, expiryTime, tokenString.Length);

            return tokenString;
        }

        public ClaimsPrincipal GetClaimsPrincipalFromExpiredToken(string token)
        {
            _logger.LogInformation("Extracting claims from expired token");

            //check meaning of this code
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
           
            var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            if (validatedToken == null)
            {
                _logger.LogWarning("Token validation returned null security token");
                return null;
            }

            var email = claimsPrincipal?.FindFirstValue(JwtRegisteredClaimNames.Email);
            var userId = claimsPrincipal?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            _logger.LogDebug("Claims extracted from token. UserId: {UserId}, Email: {Email}", userId, email);

            return claimsPrincipal;
        }
    }
}
