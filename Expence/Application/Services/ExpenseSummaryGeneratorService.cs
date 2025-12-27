using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Expence.Infrastructure.Interface;
using System.Text;
using System.Text.Json;

namespace Expence.Application.Services
{
    public class ExpenseSummaryGeneratorService : IExpenseSummaryGeneratorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly HttpClient _httpClient;
        private readonly ILogger<ExpenseSummaryGeneratorService> _logger;
        private readonly IConfiguration _config;
        public ExpenseSummaryGeneratorService(IUnitOfWork unitOfWork,
            HttpClient httpClient,
            ILogger<ExpenseSummaryGeneratorService> logger,
            IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _httpClient = httpClient;
            _logger = logger;
            _config = config;
        }
        
        public async Task<string> GenerateMonthlyReportAsync(long userId)
        {
           
            var transactions = await _unitOfWork.Transactions.GetAllMonthlyTransactionByUserIdAsync(userId);

            if (!transactions.Any())
                return "No transactions this month yet";

            var categoryBreakdown = transactions
                .GroupBy(t => t.Category)
                .Select(g => $"- {g.Key}: ${g.Sum(t => t.Amount):F2} ({g.Count()} transactions)")
                .ToList();

            var prompt = $"""
            Generate a friendly, personalized monthly expense report summary based on this data:
            
            Total Transactions: {transactions.Count}
            Total Spent: ${transactions.Sum(t => t.Amount):F2}
            Average Transaction: ${transactions.Average(t => (double)t.Amount):F2}
            
            Category Breakdown:
            {string.Join("\n", categoryBreakdown)}
            
            Please provide:
            1. A brief overview of spending patterns
            2. One key insight
            3. One actionable recommendation
            
            Keep it concise (2-3 sentences per section).
            """;

            return await CallOpenAiAsync(prompt);
        }

        private async Task<string> CallOpenAiAsync(string prompt)
        {
            try
            {
                var request = new
                {
                    model = _config["AiSettings:OpenAiModel"] ?? "gpt-3.5-turbo",
                    messages = new[]
                    {
                        new { role = "system", content = "You are a friendly financial advisor." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7,
                    max_tokens = 200
                };

                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["AiSettings:OpenAiApiKey"]}");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenAI API failed: {Status}", response.StatusCode);
                    return "Unable to generate report at this time.";
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                return doc
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString() ?? "Report generation failed.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate AI report");
                return "Unable to generate report.";
            }
        }
    }
}



//use httpclient factory 
// send mail notification 
//frontend