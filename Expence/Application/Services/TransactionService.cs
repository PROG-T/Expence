using Expence.Application.Interface;
using Expence.Domain.Constants;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Expence.Infrastructure.Interface;
using System.Security.Cryptography;

namespace Expence.Application.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserContextService _userContext;

        public TransactionService(ITransactionRepository transactionRepository, IUserRepository userRepository, IUserContextService userContext)
        {

            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
            _userContext = userContext;
        }
        public async Task<BaseResponse<Transaction>> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(_userContext.GetUserEmailAsync());
            if (user == null) return new BaseResponse<Transaction>(false, "User not found");

            var transaction = new Transaction
            {
                Amount = request.Amount,
                Description = request.Description,
                Category = request.Category,
                CreatedAt = DateTimeConstants.CurrentWestAfricanTime,
                Type = request.Type,
                UserId =  Convert.ToInt64(_userContext.GetUserIdAsync()),
                TransactionReference = $"TRX-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(100000, 999999)}"
            
            };

            await _transactionRepository.AddTransactionAsync(transaction);
            return new BaseResponse<Transaction>(true, "transaction successfully recorded");
        }

        public async Task<BaseResponse<Transaction>> DeleteTransactionAsync(string transactionReference)
        {
            var savedTransaction = await _transactionRepository.GetByTransactionReferenceAsync(transactionReference);
            if (savedTransaction == null) new BaseResponse<Transaction>(false, "transaction not found");

            await _transactionRepository.DeleteTransactionAsync(savedTransaction);
            return new BaseResponse<Transaction>(true, "transaction successfully deleted");

        }

        public async Task<BaseResponse<Transaction>> GetTransactionByIdAsync(long id)
        {
            var transaction = await _transactionRepository.GetByTransactionIdAsync(id);
            return new BaseResponse<Transaction>(true, "", transaction);
        }

        public async Task<BaseResponse<List<Transaction>>> GetAllTransactionByUserIdAsync(long userId)
        {
            var transactions = await _transactionRepository.GetAllTransactionForUserByUserIdAsync(userId);
            return new BaseResponse<List<Transaction>>(true, "", transactions);
        }

        public async Task<BaseResponse<Transaction>> UpdateTransaction(UpdateTransactionRequest transactionRequest)
        {
            var foundTransaction = await _transactionRepository.GetByTransactionReferenceAsync(transactionRequest.transactionReference);
            if (foundTransaction == null) return new BaseResponse<Transaction>(false,"No transaction found");

            foundTransaction.Amount = transactionRequest.Amount;
            foundTransaction.Category = transactionRequest.Category;
            foundTransaction.Description = transactionRequest.Description;
            foundTransaction.Type = transactionRequest.Type;

            await _transactionRepository.UpdateTransactionAsync(foundTransaction);
            return new BaseResponse<Transaction>(true, " transaction updated");
        }
    }
}

