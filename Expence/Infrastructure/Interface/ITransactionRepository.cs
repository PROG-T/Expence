using Expence.Domain.Models;

namespace Expence.Infrastructure.Interface
{
    public interface ITransactionRepository
    {
        Task<Transaction> GetByTransactionIdAsync(long id);
        Task<IList<Transaction>> GetAllTransactionForUserByUserIdAsync(long userid);
        Task AddAsync(Transaction transaction);
        Task DeleteAsync(Transaction transaction);
    }
}
