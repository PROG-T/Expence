using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expence.Controllers
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = await _transactionService.CreateTransactionAsync( request);

            if (!response.Status)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUserTransactions()
        {
            var response = await _transactionService.GetAllTransactionByUserIdAsync(Convert.ToInt64(_userContext.GetUserIdAsync()));
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionByTransactionId([FromQuery]long transactionId)
        {
            var response = await _transactionService.GetTransactionByIdAsync(transactionId);

            if (!response.Status)
                return NotFound(response);

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction([FromBody] UpdateTransactionRequest request)
        {
            var response = await _transactionService.UpdateTransaction(request );

            if (!response.Status)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var response = await _transactionService.DeleteTransactionAsync(id);

            if (!response.Status)
                return NotFound(response);

            return Ok(response);
        }
    }
}
