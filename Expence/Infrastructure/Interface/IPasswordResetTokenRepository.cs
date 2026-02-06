using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken> GetValidTokenAsync(string token, long userId);
        Task<PasswordResetToken> GetByTokenAsync(string token);
        Task AddTokenAsync(PasswordResetToken token);
        Task UpdateTokenAsync(PasswordResetToken token);
        Task InvalidateAllTokensAsync(long userId);
    }
}
