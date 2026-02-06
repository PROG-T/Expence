using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);
        Task RevokeAllUserTokensAsync(long userId);
        Task RemoveExpiredTokensAsync();
    }
}
