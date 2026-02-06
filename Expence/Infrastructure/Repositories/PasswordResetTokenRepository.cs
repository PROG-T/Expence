using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository: IPasswordResetTokenRepository
    {
        private readonly ExpenceDbContext _context;

        public PasswordResetTokenRepository(ExpenceDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken> GetValidTokenAsync(string token, long userId)
        {
            return await _context.PasswordResetTokens
                .Where(t => t.Token == token && t.UserId == userId && !t.IsUsed && t.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<PasswordResetToken> GetByTokenAsync(string token)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task AddTokenAsync(PasswordResetToken token)
        {
            await _context.PasswordResetTokens.AddAsync(token);
        }

        public async Task UpdateTokenAsync(PasswordResetToken token)
        {
            _context.PasswordResetTokens.Update(token);
            await Task.CompletedTask;
        }

        public async Task InvalidateAllTokensAsync(long userId)
        {
            var tokens = await _context.PasswordResetTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }
            _context.PasswordResetTokens.UpdateRange(tokens);
        }
    }
}
