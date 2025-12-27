using Expence.Application.Interface;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Expence.Application.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        public UserContextService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public string GetUserEmail()
        {
            return _contextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Email);
        }

        public string GetUserId()
        {
            return _contextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }
    }
}
