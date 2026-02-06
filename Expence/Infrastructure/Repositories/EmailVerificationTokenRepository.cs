using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure.Repositories
{
    public class EmailVerificationTokenRepository
    : IEmailVerificationTokenRepository
    {
        private readonly ExpenceDbContext _context;

        public EmailVerificationTokenRepository(ExpenceDbContext context)
        {
            _context = context;
        }

        public async Task<EmailVerificationToken> GetValidTokenAsync(string token, long userId)
        {
            return await _context.EmailVerificationTokens
                .Where(t => t.Token == token && t.UserId == userId && !t.IsUsed && t.ExpiryDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<EmailVerificationToken> GetByTokenAsync(string token)
        {
            return await _context.EmailVerificationTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task AddTokenAsync(EmailVerificationToken token)
        {
            await _context.EmailVerificationTokens.AddAsync(token);
        }

        public async Task UpdateTokenAsync(EmailVerificationToken token)
        {
            _context.EmailVerificationTokens.Update(token);
            await Task.CompletedTask;
        }

        public async Task InvalidateAllTokensAsync(long userId)
        {
            var tokens = await _context.EmailVerificationTokens
                .Where(t => t.UserId == userId && !t.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            _context.EmailVerificationTokens.UpdateRange(tokens);
        }
    }

}
