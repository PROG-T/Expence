using Azure.Core;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
        }

        public void DeleteTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Remove(transaction);
        }

        public async Task<List<Transaction>> GetAllMonthlyTransactionByUserIdAsync(long userId)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

            var startOfNextMonth = startOfMonth.AddMonths(1);

            return await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.CreatedAt >= startOfMonth &&
                    t.CreatedAt < startOfNextMonth)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<PagedResult<Transaction>> GetAllTransactionForUserByUserIdAsync(TransactionQueryRequest request)
        {
            var transactions = _context.Transactions.AsNoTracking().Where(t => t.UserId == request.UserId);

            if (!string.IsNullOrWhiteSpace(request.Category))
                transactions = transactions.Where(t => t.Category == request.Category);

            if (!string.IsNullOrWhiteSpace(request.Type.ToString()))
                transactions = transactions.Where(t => t.Type == request.Type);

            if (request.FromDate.HasValue)
                transactions = transactions.Where(t => t.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                transactions = transactions.Where(t => t.CreatedAt <= request.ToDate.Value);

            var ItemCount = await transactions.CountAsync();

            var items = await transactions.OrderByDescending(t => t.CreatedAt).Skip((request.Page-1) * request.PageSize).Take(request.PageSize).ToListAsync();

            return new PagedResult<Transaction>
            {
                Items = items,
                TotalCount = ItemCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
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

        }
    }
}
