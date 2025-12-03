using Expence.Domain.DTOs;
using Expence.Domain.Models;

namespace Expence.Application.Interface
{
    public interface ITransactionService
    {
        Task<BaseResponse<Transaction>> CreateTransactionAsync(CreateTransactionRequest transaction);
        Task<BaseResponse<Transaction>> DeleteTransactionAsync(string transactionReference);
        Task<BaseResponse<Transaction>> UpdateTransaction(UpdateTransactionRequest transaction);
        Task<BaseResponse<Transaction>> GetTransactionByIdAsync(long id);
        Task<BaseResponse<List<Transaction>>> GetAllTransactionByUserIdAsync(long userId);
    }
}
