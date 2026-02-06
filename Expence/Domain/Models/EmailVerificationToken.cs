using Expence.Domain.Constants;

namespace Expence.Domain.Models
{
    public class EmailVerificationToken
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTimeConstants.CurrentWestAfricanTime;
    }
}
