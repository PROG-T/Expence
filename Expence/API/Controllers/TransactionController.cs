using Asp.Versioning;
using Expence.Application.Attributes;
using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Expence.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [EnableRateLimiting("transaction")]
    public class TransactionController: ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IUserContextService _userContext;

        public TransactionController(ITransactionService transactionService, IUserContextService userContext)
        {
            _transactionService = transactionService;
            _userContext = userContext;
        }

        /// <summary>
        /// Creates a new transaction for the authenticated user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {

            var response = await _transactionService.CreateTransactionAsync( request);

            if (!response.Status)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Retrieves all transactions for the authenticated user with optional filtering and pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUserTransactions([FromQuery] TransactionQueryParameters request)
        {
            var queryRequest = new TransactionQueryRequest
            {
                UserId = Convert.ToInt64(_userContext.GetUserId().Data),
                Category = request.Category,
                Type = request.Type,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Page = request.Page,
                PageSize = request.PageSize
            };
            var response = await _transactionService.GetAllTransactionByUserIdAsync(queryRequest);
            if (!response.Status)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Retrieves a specific transaction by its reference for the authenticated user.
        /// </summary>
        [HttpGet("{reference}")]
        [ValidateTransactionOwnership]
        public async Task<IActionResult> GetTransactionByTransactionReference(string reference)
        {
            var response = await _transactionService.GetTransactionByTransactionReference(reference);

            if (!response.Status)
                return NotFound(response);

            return Ok(response);
        }

        /// <summary>
        /// Updates a specific transaction for the authenticated user.
        /// </summary>
        [HttpPut("{reference}")]
        [ValidateTransactionOwnership]
        public async Task<IActionResult> UpdateTransaction(string reference, [FromBody] UpdateTransactionRequest request)
        {
            var response = await _transactionService.UpdateTransaction(request );

            if (!response.Status)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Deletes a specific transaction for the authenticated user.
        /// </summary>
        [HttpDelete("{reference}")]
        [ValidateTransactionOwnership]
        public async Task<IActionResult> Delete(string reference)
        {
            var response = await _transactionService.DeleteTransactionAsync(reference);

            if (!response.Status)
                return NotFound(response);

            return Ok(response);
        }
    }
}