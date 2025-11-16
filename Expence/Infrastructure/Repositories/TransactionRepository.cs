using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;

namespace Expence.Infrastructure.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ExpenceDbContext _context;
        public TransactionRepository(ExpenceDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await  _context.SaveChangesAsync();
        }

        public Task DeleteAsync(Transaction transaction)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Transaction>> GetAllTransactionForUserByUserIdAsync(long userId)
        {
             return await _context.Transactions.Where(t => t.UserId == userId).ToListAsync()       }

        public async Task<Transaction> GetByTransactionIdAsync(long id)
        {
            return await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
