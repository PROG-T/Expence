using Expence.Application.Interface;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Expence.Application.Services
{
    /// <summary>
    /// Sliding window rate limiter implementation using Redis
    /// Uses userId as partition key for authenticated requests, IP for unauthenticated
    /// </summary>
    public class SlidingWindowRateLimiter : ISlidingWindowRateLimiter
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<SlidingWindowRateLimiter> _logger;

        public SlidingWindowRateLimiter(IDistributedCache cache, ILogger<SlidingWindowRateLimiter> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Generates a partition key based on userId or IP address
        /// </summary>
        public string GeneratePartitionKey(string userId, string clientIp, string endpoint)
        {
            // For authenticated requests, use userId as partition key
            if (!string.IsNullOrEmpty(userId))
            {
                return $"ratelimit:user:{userId}:{endpoint}";
            }

            // For unauthenticated requests, use IP address
            return $"ratelimit:ip:{clientIp}:{endpoint}";
        }

        public async Task<RateLimitResult> CheckRateLimitAsync(
            string partitionKey,
            int permitLimit,
            int windowSizeInSeconds)
        {
            try
            {
                var now = DateTime.UtcNow;
                var windowStart = now.AddSeconds(-windowSizeInSeconds);

                _logger.LogDebug(
                    "Checking rate limit. PartitionKey: {PartitionKey}, Limit: {Limit}, Window: {WindowSize}s",
                    partitionKey, permitLimit, windowSizeInSeconds);

                // Get request history from cache
                var requestsJson = await _cache.GetStringAsync(partitionKey);
                var requests = string.IsNullOrEmpty(requestsJson)
                    ? new List<DateTime>()
                    : JsonSerializer.Deserialize<List<DateTime>>(requestsJson) ?? new List<DateTime>();

                // Remove old requests outside the window
                var validRequests = requests
                    .Where(r => r > windowStart)
                    .ToList();

                // Check if limit exceeded
                if (validRequests.Count >= permitLimit)
                {
                    var oldestRequest = validRequests.First();
                    var retryAfter = (oldestRequest.AddSeconds(windowSizeInSeconds) - now).TotalSeconds;

                    _logger.LogWarning(
                        "Rate limit exceeded. PartitionKey: {PartitionKey}, Requests: {Count}/{Limit}, RetryAfter: {RetryAfter}s",
                        partitionKey, validRequests.Count, permitLimit, Math.Ceiling(retryAfter));

                    return new RateLimitResult
                    {
                        IsAllowed = false,
                        RetryAfter = TimeSpan.FromSeconds(Math.Ceiling(retryAfter)),
                        RequestCount = validRequests.Count,
                        PermitLimit = permitLimit,
                        PartitionKey = partitionKey
                    };
                }

                // Add current request
                validRequests.Add(now);

                // Save updated request history (with expiration = window size + buffer)
                var updatedJson = JsonSerializer.Serialize(validRequests);
                await _cache.SetStringAsync(
                    partitionKey,
                    updatedJson,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(windowSizeInSeconds + 10)
                    });

                _logger.LogDebug(
                    "Rate limit check passed. PartitionKey: {PartitionKey}, Requests: {Count}/{Limit}",
                    partitionKey, validRequests.Count, permitLimit);

                return new RateLimitResult
                {
                    IsAllowed = true,
                    RequestCount = validRequests.Count,
                    PermitLimit = permitLimit,
                    PartitionKey = partitionKey
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking rate limit for partition key: {PartitionKey}", partitionKey);
                // Fail open - allow request if rate limiter fails
                return new RateLimitResult { IsAllowed = true };
            }
        }
    }

    public class RateLimitResult
    {
        public bool IsAllowed { get; set; }
        public TimeSpan? RetryAfter { get; set; }
        public int RequestCount { get; set; }
        public int PermitLimit { get; set; }
        public string PartitionKey { get; set; }
    }
}