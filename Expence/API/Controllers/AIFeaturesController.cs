using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expence.API.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    [Authorize]
    public class AIFeaturesController : ControllerBase
    {
        private readonly ICategoryPredictionService _categoryPredictionService;
        private readonly IUserContextService _userContext;
        private readonly IExpenseSummaryGeneratorService _expenseSummaryGeneratorService;

        public AIFeaturesController(ICategoryPredictionService categoryPredictionService, IUserContextService userContextService, IExpenseSummaryGeneratorService expenseSummaryGeneratorService)
        {
            _categoryPredictionService = categoryPredictionService;
            _userContext = userContextService;
            _expenseSummaryGeneratorService = expenseSummaryGeneratorService;
        }

        [HttpPost("predict-category")]
        public async Task<IActionResult> PredictCategory([FromBody] PredictCategoryRequest request)
        {
            var categoryResponse = await _categoryPredictionService.PredictCategoryAsync(request.Description, request.Type);

            return Ok(categoryResponse);
        }

        [HttpGet("monthly-report")]
        public async Task<IActionResult> GetMonthlyReport()
        {
            var userId = Convert.ToInt64(_userContext.GetUserId());
            var report = await _expenseSummaryGeneratorService.GenerateMonthlyReportAsync(userId);
            return Ok(report);
        }

    }
}
