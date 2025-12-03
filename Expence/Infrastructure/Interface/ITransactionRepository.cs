using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByTransactionIdAsync(long id);
        Task<Transaction> GetByTransactionReferenceAsync(string transactionReference);
        Task<List<Transaction>> GetAllTransactionForUserByUserIdAsync(long userid);
        Task AddTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(Transaction transaction);
        Task UpdateTransactionAsync(Transaction transaction);
    }
}
