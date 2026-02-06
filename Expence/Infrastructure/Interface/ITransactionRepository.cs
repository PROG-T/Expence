using Expence.Domain.DTOs;
using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByTransactionIdAsync(long id);
        Task<Transaction> GetByTransactionReferenceAsync(string transactionReference);
        Task<PagedResult<Transaction>> GetAllTransactionForUserByUserIdAsync(TransactionQueryRequest request);
        Task<List<Transaction>> GetAllMonthlyTransactionByUserIdAsync(long userId);
        Task AddTransactionAsync(Transaction transaction);
        void DeleteTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
