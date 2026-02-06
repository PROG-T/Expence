using Expence.Application.Services;

namespace Expence.Application.Interface
{
    public interface ISlidingWindowRateLimiter
    {
        string GeneratePartitionKey(string userId, string clientIp, string endpoint);

        Task<RateLimitResult> CheckRateLimitAsync(
            string key,
            int permitLimit,
            int windowSizeInSeconds);
    }
}
