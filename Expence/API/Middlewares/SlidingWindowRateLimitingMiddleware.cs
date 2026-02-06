using Expence.Application.Interface;
using Expence.Domain.OptionsConfiguration;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Expence.API.Middlewares
{
    public class SlidingWindowRateLimitingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SlidingWindowRateLimitingMiddleware> _logger;
        private readonly RateLimitingOptions _rateLimitingOptions;
        public SlidingWindowRateLimitingMiddleware(
           RequestDelegate next,
           ILogger<SlidingWindowRateLimitingMiddleware> logger,
           IOptions<RateLimitingOptions> rateLimitingOptions)
        {
            _next = next;
            _logger = logger;
            _rateLimitingOptions = rateLimitingOptions.Value;
        }
        public async Task InvokeAsync(HttpContext context, ISlidingWindowRateLimiter rateLimiter)
        {
            var path = context.Request.Path.ToString();
            var method = context.Request.Method;

            // Extract userId from JWT token (if authenticated)
            var userId = context.User?.FindFirstValue("sub") ??
                        context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var endpoint = GetEndpointKey(path);

            // Generate partition key (userId for authenticated, IP for unauthenticated)
            var partitionKey = rateLimiter.GeneratePartitionKey(userId, clientIp,endpoint);

            // Get rate limit config based on endpoint type
            var (permitLimit, windowSize) = GetRateLimitConfig(path, userId);

            _logger.LogDebug(
                "Rate limit check. Endpoint: {Endpoint}, Method: {Method}, PartitionKey: {PartitionKey}, " +
                "Authenticated: {IsAuthenticated}, ClientIp: {ClientIp}",
                path, method, partitionKey, !string.IsNullOrEmpty(userId), clientIp);

            // Check rate limit
            var result = await rateLimiter.CheckRateLimitAsync(partitionKey, permitLimit, windowSize);

            if (!result.IsAllowed)
            {
                _logger.LogWarning(
                    "Rate limit exceeded. Endpoint: {Endpoint}, PartitionKey: {PartitionKey}, " +
                    "Requests: {Count}/{Limit}, RetryAfter: {RetryAfter}s",
                    path, partitionKey, result.RequestCount, result.PermitLimit,
                    result.RetryAfter?.TotalSeconds);

                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.Response.ContentType = "application/json";

                if (result.RetryAfter.HasValue)
                {
                    context.Response.Headers.Add("Retry-After",
                        ((int)result.RetryAfter.Value.TotalSeconds).ToString());
                }

                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Too many requests. Please try again later.",
                    retryAfterSeconds = (int?)result.RetryAfter?.TotalSeconds,
                    statusCode = 429
                });

                return;
            }

            // Add rate limit info to response headers
            context.Response.Headers.Add("X-RateLimit-Limit", result.PermitLimit.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining",
                Math.Max(0, result.PermitLimit - result.RequestCount).ToString());
            context.Response.Headers.Add("X-RateLimit-Reset",
                DateTimeOffset.UtcNow.AddSeconds(windowSize).ToUnixTimeSeconds().ToString());

            _logger.LogDebug(
                "Rate limit check passed. PartitionKey: {PartitionKey}, Remaining: {Remaining}/{Limit}",
                partitionKey, result.PermitLimit - result.RequestCount, result.PermitLimit);

            await _next(context);
        }
        private string GetEndpointKey(string path)
        {
            // Normalize path to group similar endpoints
            if (path.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase))
                return "/api/v1/auth";

            if (path.StartsWith("/api/v1/transaction", StringComparison.OrdinalIgnoreCase))
                return "/api/v1/transaction";

            if (path.StartsWith("/api/v1/aifeatures", StringComparison.OrdinalIgnoreCase))
                return "/api/v1/aifeatures";

            return path;
        }
        private (int permitLimit, int windowSize) GetRateLimitConfig(string path, string userId)
        {
            // Auth endpoints are stricter per-IP for unauthenticated
            if (path.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    _rateLimitingOptions.Auth.PermitLimit,
                    _rateLimitingOptions.Auth.WindowSizeInSeconds
                );
            }

            // AI features are more restrictive per-user due to API costs
            if (path.StartsWith("/api/v1/aifeatures", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    _rateLimitingOptions.AiFeatures.PermitLimit,
                    _rateLimitingOptions.AiFeatures.WindowSizeInSeconds
                );
            }

            // Transaction endpoints
            if (path.StartsWith("/api/v1/transaction", StringComparison.OrdinalIgnoreCase))
            {
                return (
                    _rateLimitingOptions.Transaction.PermitLimit,
                    _rateLimitingOptions.Transaction.WindowSizeInSeconds
                );
            }

            // Default global
            return (
                _rateLimitingOptions.Global.PermitLimit,
                _rateLimitingOptions.Global.WindowSizeInSeconds
            );
        }
    

}
}
