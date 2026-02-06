using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface IEmailVerificationTokenRepository
    {
        Task<EmailVerificationToken> GetValidTokenAsync(string token, long userId);
        Task<EmailVerificationToken> GetByTokenAsync(string token);
        Task AddTokenAsync(EmailVerificationToken token);
        Task UpdateTokenAsync(EmailVerificationToken token);
        Task InvalidateAllTokensAsync(long userId);
    }
}
