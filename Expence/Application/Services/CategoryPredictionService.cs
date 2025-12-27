using Expence.Application.Interface;
using Expence.Domain.Enum;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Expence.Application.Services
{
    public class CategoryPredictionService : ICategoryPredictionService
    {
        
        private readonly HttpClient _httpClient;
        private readonly ILogger<CategoryPredictionService> _logger;
        private readonly IConfiguration _config;

        private static readonly string[] AllowedCategories =
        {
            "Food", "Transport", "Entertainment", "Health", "Shopping", "Bills", "Other"
        };

        public CategoryPredictionService(HttpClient httpClient, ILogger<CategoryPredictionService> logger, IConfiguration config)
        {
            _httpClient = httpClient;
            _logger = logger;
            _config = config;

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", config["AiSettings:OpenAiApiKey"]);
        }

        public async Task<string> PredictCategoryAsync(string description, TransactionType type)
        {
            try
            {
                var prompt = $"""
            You are an expense categorization assistant.

            Transaction description:
            "{description}"

            Transaction type:
            "{type.ToString()}"

            Choose ONE category strictly from this list:
            {string.Join(", ", AllowedCategories)}

            Respond with ONLY the category name.
            """;

                var request = new
                {
                    model = _config["AiSettings:OpenAiApiKey"],
                    input = prompt
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync("responses", content);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenAI request failed: {Status}", response.StatusCode);
                    return "Other";
                }

                var json = await response.Content.ReadAsStringAsync();
                return ExtractCategory(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Category prediction failed");
                return "Other";
            }
        }

        private string ExtractCategory(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);

                var text = doc
                    .RootElement
                    .GetProperty("output")[0]
                    .GetProperty("content")[0]
                    .GetProperty("text")
                    .GetString();

                if (string.IsNullOrWhiteSpace(text))
                    return "Other";

                var normalized = text.Trim();

                return AllowedCategories.Contains(normalized) ? normalized : "Other";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse OpenAI response");
                return "Other";
            }
        }
    }

}
