using Expence.Application.Interface;
using Expence.Domain.Constants;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Expence.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContextService _userContext;

        public TransactionService(IUnitOfWork unitOfWork, IUserContextService userContext)
        {

          _unitOfWork = unitOfWork;
          _userContext = userContext;
        }
        public async Task<BaseResponse<Transaction>> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var user = await _unitOfWork.Users.GetUserByEmailAsync(_userContext.GetUserEmail());
            if (user == null) return new BaseResponse<Transaction>(false, "User not found");

            var transaction = new Transaction
            {
                Amount = request.Amount,
                Description = request.Description,
                Category = request.Category,
                CreatedAt = DateTimeConstants.CurrentWestAfricanTime,
                Type = request.Type,
                UserId =  Convert.ToInt64(_userContext.GetUserId()),
                TransactionReference = $"TRX-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(100000, 999999)}"
            
            };

            await _unitOfWork.Transactions.AddTransactionAsync(transaction);
            await _unitOfWork.SaveAsync();

            return new BaseResponse<Transaction>(true, "transaction successfully recorded");
        }

        public async Task<BaseResponse<Transaction>> DeleteTransactionAsync(string transactionReference, long userId)
        {
            var savedTransaction = await _unitOfWork.Transactions.GetByTransactionReferenceAsync(transactionReference);
            if (savedTransaction == null || savedTransaction.UserId != userId) 
                return new BaseResponse<Transaction>(false, "transaction not found");

             _unitOfWork.Transactions.DeleteTransactionAsync(savedTransaction);
            await _unitOfWork.SaveAsync();

            return new BaseResponse<Transaction>(true, "transaction successfully deleted");

        }

        public async Task<BaseResponse<Transaction>> GetTransactionByIdAsync(long id)
        {
            var transaction = await _unitOfWork.Transactions.GetByTransactionIdAsync(id);
            return new BaseResponse<Transaction>(true, "", transaction);
        }

        public async Task<BaseResponse<List<Transaction>>> GetAllTransactionByUserIdAsync(TransactionQueryRequest request)
        {
            var transactions = await _unitOfWork.Transactions.GetAllTransactionForUserByUserIdAsync(request);
            return new BaseResponse<List<Transaction>>(true, "", transactions);
        }

        public async Task<BaseResponse<Transaction>> UpdateTransaction(UpdateTransactionRequest transactionRequest)
        {
            var foundTransaction = await _unitOfWork.Transactions.GetByTransactionReferenceAsync(transactionRequest.transactionReference);
            if (foundTransaction == null) return new BaseResponse<Transaction>(false,"No transaction found");

            foundTransaction.Amount = transactionRequest.Amount;
            foundTransaction.Category = transactionRequest.Category;
            foundTransaction.Description = transactionRequest.Description;
            foundTransaction.Type = transactionRequest.Type;

            await _unitOfWork.Transactions.UpdateTransactionAsync(foundTransaction);
            await _unitOfWork.SaveAsync();

            return new BaseResponse<Transaction>(true, " transaction updated");
        }
    }
}

