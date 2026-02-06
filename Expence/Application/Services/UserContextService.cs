using Expence.Application.Interface;
using Expence.Domain.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Expence.Application.Services
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<UserContextService> _logger;

        public UserContextService(IHttpContextAccessor contextAccessor, ILogger<UserContextService> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }
        public BaseResponse<string> GetUserEmail()
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext?.User == null)
            {
                _logger.LogWarning("HttpContext or User is null when retrieving email");
                return new BaseResponse<string>( false, "user object not found in context" );
            }

            var email = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Email);

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("Email claim not found in current user context");
                return new BaseResponse<string>(false, "user mail not found in context");
            }

            _logger.LogDebug("Retrieved user email from context: {Email}", email);
            return new BaseResponse<string>(true, email);
        }

        public BaseResponse<string> GetUserId()
        {
            var httpContext = _contextAccessor.HttpContext;
            if (httpContext?.User == null)
            {
                _logger.LogWarning("HttpContext or User is null when retrieving UserId");
                return new BaseResponse<string>(false, "user object not found in context");
            }

            var userId = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId claim (Sub) not found in current user context");
                return new BaseResponse<string>(false, "user id not found in context");
            }

            _logger.LogDebug("Retrieved user ID from context: {UserId}", userId);
            return new BaseResponse<string>(true, userId);
        }
    }
}
