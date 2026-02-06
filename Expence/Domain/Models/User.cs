namespace Expence.Domain.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public IList<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public IList<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();

    }
}
