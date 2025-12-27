using Expence.Application.Attributes;
using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Expence.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expence.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class TransactionController: ControllerBase
    {
        private readonly ITransactionService _transactionService;
        private readonly IUserContextService _userContext;

        public TransactionController(ITransactionService transactionService, IUserContextService userContext)
        {
            _transactionService = transactionService;
            _userContext = userContext;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {

            var response = await _transactionService.CreateTransactionAsync( request);

            if (!response.Status)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserTransactions([FromQuery] TransactionQueryParameters request)
        {
            var queryRequest = new TransactionQueryRequest
            {
                UserId = Convert.ToInt64(_userContext.GetUserId()),
                Category = request.Category,
                Type = request.Type,
                FromDate = request.FromDate,
                ToDate = request.ToDate,
                Page = request.Page,
                PageSize = request.PageSize
            };
            var response = await _transactionService.GetAllTransactionByUserIdAsync(queryRequest);
            return Ok(response);
        }

        [HttpGet("{reference}")]
        [ValidateTransactionOwnership]
        public async Task<IActionResult> GetTransactionByTransactionId(long reference)
        {
            var response = await _transactionService.GetTransactionByIdAsync(reference);

            if (!response.Status)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("{reference}")]
        [ValidateTransactionOwnership]
        public async Task<IActionResult> UpdateTransaction(long reference, [FromBody] UpdateTransactionRequest request)
        {
            var response = await _transactionService.UpdateTransaction(request );

            if (!response.Status)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{reference}")]
        [ValidateTransactionOwnership]
        public async Task<IActionResult> Delete(string reference)
        {
            var response = await _transactionService.DeleteTransactionAsync(reference, Convert.ToInt64(_userContext.GetUserId()));

            if (!response.Status)
                return NotFound(response);

            return Ok(response);
        }
    }
}