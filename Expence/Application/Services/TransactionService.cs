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
        private readonly ILogger<TransactionService> _logger;

        public TransactionService(IUnitOfWork unitOfWork, IUserContextService userContext, ILogger<TransactionService> logger)
        {

            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger;
        }
        public async Task<BaseResponse<Transaction>> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var userEmail = _userContext.GetUserEmail().Data;
            var userId = _userContext.GetUserId().Data;

            _logger.LogInformation("Creating transaction for user: {UserId} ({Email})", userId, userEmail);

            var user = await _unitOfWork.Users.GetUserByEmailAsync(userEmail);
            if (user == null)
            {
                _logger.LogWarning("User not found for email: {Email}", userEmail);
                return new BaseResponse<Transaction>(false, "User not found");
            }

            var transaction = new Transaction
            {
                Amount = request.Amount,
                Description = request.Description,
                Category = request.Category,
                CreatedAt = DateTimeConstants.CurrentWestAfricanTime,
                Type = request.Type,
                UserId =  Convert.ToInt64(_userContext.GetUserId().Data),
                TransactionReference = $"TRX-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(100000, 999999)}"
            
            };

            await _unitOfWork.Transactions.AddTransactionAsync(transaction);
            await _unitOfWork.SaveAsync();
            _logger.LogInformation(  "Transaction created successfully. Reference: {Reference}, Type: {Type}, Amount: {Amount}, Category: {Category}, UserId: {UserId}",
                   transaction.TransactionReference, request.Type, request.Amount, request.Category, userId);
            return new BaseResponse<Transaction>(true, "transaction successfully recorded");
        }

        public async Task<BaseResponse<Transaction>> DeleteTransactionAsync(string transactionReference)
        {
            _logger.LogInformation("Attempting to delete transaction. Reference: {Reference}", transactionReference);

            var savedTransaction = await _unitOfWork.Transactions.GetByTransactionReferenceAsync(transactionReference);
            if (savedTransaction == null)
            {
                _logger.LogWarning("Transaction not found. Reference: {Reference}", transactionReference);

                return new BaseResponse<Transaction>(false, "transaction not found");
            }
             _unitOfWork.Transactions.DeleteTransactionAsync(savedTransaction);
            await _unitOfWork.SaveAsync();

            return new BaseResponse<Transaction>(true, "transaction successfully deleted");

        }

        public async Task<BaseResponse<PagedResult<Transaction>>> GetAllTransactionByUserIdAsync(TransactionQueryRequest request)
        {
            _logger.LogInformation(
                    "Retrieving transactions for user. UserId: {UserId}, Page: {Page}, PageSize: {PageSize}, Category: {Category}, Type: {Type}",
                    request.UserId, request.Page, request.PageSize, request.Category, request.Type);
           
            if (request.Page < 1)
                request.Page = 1;
            if (request.PageSize < 1)
                request.PageSize = 10;
            if (request.PageSize > 100)
                request.PageSize = 100;

            var transactions = await _unitOfWork.Transactions.GetAllTransactionForUserByUserIdAsync(request);
            _logger.LogInformation(
                    "Transactions retrieved successfully for user. UserId: {UserId}, Total Records: {TotalRecords}, Returned: {ReturnedCount}",
                    request.UserId, transactions.Items, transactions.TotalCount);

            return new BaseResponse<PagedResult<Transaction>>(true, "", transactions);
        }

        public async Task<BaseResponse<Transaction>> UpdateTransaction(UpdateTransactionRequest transactionRequest)
        {
            var userId = _userContext.GetUserId().Data;

            _logger.LogInformation(
                "Updating transaction. Reference: {Reference}, UserId: {UserId}",
                transactionRequest.transactionReference, userId);

            var foundTransaction = await _unitOfWork.Transactions.GetByTransactionReferenceAsync(transactionRequest.transactionReference);
            if (foundTransaction == null)
            {
                _logger.LogWarning("Transaction not found for update. Reference: {Reference}", transactionRequest.transactionReference);
                return new BaseResponse<Transaction>(false, "No transaction found"); 
            }

            _logger.LogDebug(
                    "Transaction previous values. Reference: {Reference}, Amount: {PrevAmount}, Category: {PrevCategory}, Type: {PrevType}",
                    foundTransaction.TransactionReference, foundTransaction.Amount, foundTransaction.Category, foundTransaction.Type);

            foundTransaction.Amount = transactionRequest.Amount;
            foundTransaction.Category = transactionRequest.Category;
            foundTransaction.Description = transactionRequest.Description;
            foundTransaction.Type = transactionRequest.Type;
            foundTransaction.ModifiedAt = DateTimeConstants.CurrentWestAfricanTime;

            await _unitOfWork.Transactions.UpdateTransactionAsync(foundTransaction);
            await _unitOfWork.SaveAsync();

            _logger.LogInformation(
                    "Transaction updated successfully. Reference: {Reference}, New Amount: {Amount}, New Category: {Category}, New Type: {Type}, UserId: {UserId}",
                    foundTransaction.TransactionReference, foundTransaction.Amount, foundTransaction.Category, foundTransaction.Type, userId);

            return new BaseResponse<Transaction>(true, " transaction updated");
        }

        public async Task<BaseResponse<Transaction>> GetTransactionByTransactionReference(string reference)
        {
            _logger.LogInformation("Retrieving transaction by reference: {Reference}", reference);

            var transaction = await _unitOfWork.Transactions.GetByTransactionReferenceAsync(reference);
            if (transaction == null)
            {
                _logger.LogWarning("Transaction not found. Reference: {Reference}", reference);
                return new BaseResponse<Transaction>(false, "Transaction not found");
            }

            _logger.LogInformation("Transaction retrieved successfully. Reference: {Reference}, UserId: {UserId}", reference, transaction.UserId);

            return new BaseResponse<Transaction>(true, "", transaction);
        }
    }
}

