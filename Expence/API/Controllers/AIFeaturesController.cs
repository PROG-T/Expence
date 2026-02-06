using Asp.Versioning;
using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Expence.API.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    [EnableRateLimiting("ai-features")]
    public class AIFeaturesController : ControllerBase
    {
        private readonly ICategoryPredictionService _categoryPredictionService;
        private readonly IUserContextService _userContext;
        private readonly IExpenseSummaryGeneratorService _expenseSummaryGeneratorService;
        private readonly ILogger<AIFeaturesController> _logger;

        public AIFeaturesController(
            ICategoryPredictionService categoryPredictionService,
            IUserContextService userContextService,
            IExpenseSummaryGeneratorService expenseSummaryGeneratorService,
            ILogger<AIFeaturesController> logger)
        {
            _categoryPredictionService = categoryPredictionService;
            _userContext = userContextService;
            _expenseSummaryGeneratorService = expenseSummaryGeneratorService;
            _logger = logger;
        }

        /// <summary>
        /// Predicts the category of a transaction based on its description.
        /// </summary>
        [HttpPost("predict-category")]
        public async Task<IActionResult> PredictCategory([FromBody] PredictCategoryRequest request)
        {
            var predictedCategory = await _categoryPredictionService.PredictCategoryAsync(request.Description, request.Type);

            var categoryResponse = new CategoryPredictionResponse
            {
                PredictedCategory = predictedCategory,
                Confidence = 0.85f, // You could add confidence scoring based on category certainty
                IsValid = !string.IsNullOrWhiteSpace(predictedCategory) && predictedCategory != "Other"
            };

            return Ok(new BaseResponse<CategoryPredictionResponse>(true, "Category prediction successful", categoryResponse));
        }

        // <summary>
        /// Generates a monthly expense report with AI-powered insights and recommendations.
        /// </summary>
        [HttpGet("monthly-report")]
        public async Task<IActionResult> GetMonthlyReport()
        {
            var userId = Convert.ToInt64(_userContext.GetUserId().Data);
            var report = await _expenseSummaryGeneratorService.GenerateMonthlyReportAsync(userId);
            var reportResponse = new AiReportResponse
            {
                Report = report,
                GeneratedAt = DateTime.UtcNow,
                Period = $"{DateTime.Now:MMMM yyyy}"
            };

            return Ok(new BaseResponse<AiReportResponse>(true, "Monthly report generated successfully", reportResponse));
        }

    }
}
