using Expence.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Expence.Application.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly HttpContextAccessor _contextAccessor;
        public UserContextService(HttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public string GetUserEmailAsync()
        {
            return _contextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }

        public string GetUserIdAsync()
        {
            return _contextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email);
        }
    }
}
