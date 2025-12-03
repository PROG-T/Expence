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
        public async Task AddTransactionAsync(Transaction transaction)
        {
            await _context.Transactions.AddAsync(transaction);
            await  _context.SaveChangesAsync();
        }

        public async Task DeleteTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Transaction>> GetAllTransactionForUserByUserIdAsync(long userId)
        {
            // add pagination
            //filter by category, type, date
            return await _context.Transactions.AsNoTracking().Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task<Transaction> GetByTransactionIdAsync(long id)
        {
            return await _context.Transactions.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Transaction> GetByTransactionReferenceAsync(string transactionReference)
        {
            return await _context.Transactions.FirstOrDefaultAsync(t => t.TransactionReference == transactionReference);
        }

        public async Task UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();

        }
    }
}
