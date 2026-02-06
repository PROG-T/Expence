using Expence.Domain.Constants;

namespace Expence.Domain.Models
{
    public class RefreshToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTimeConstants.CurrentWestAfricanTime;

        // Foreign key
        public User User { get; set; } = null!;
    }
}
