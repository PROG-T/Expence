namespace Expence.Domain.DTOs
{
    public class RateLimitExceededResponse
    {
        public string Message { get; set; } = "Too many requests. Please try again later.";

        public int RetryAfterSeconds { get; set; }

        public string RetryAfter { get; set; } = "";
    }
}
