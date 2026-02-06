using Expence.Application.Interface;
using Expence.Domain.DTOs;
using Expence.Domain.Enum;
using System.Net.Http;
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
        }

        public async Task<string> PredictCategoryAsync(string description, TransactionType type)
        {
            try
            {
                _logger.LogInformation("Category prediction requested. Description: {Description}, Type: {Type}",
                    description, type);

                var prompt = $"""
            You are an expense categorization assistant.

            Transaction description:  "{description}"

            Transaction type:  "{type.ToString()}"

            Choose ONE category strictly from this list:   {string.Join(", ", AllowedCategories)}

            Respond with ONLY the category name.
            """;

                var request = new
                {
                    model = _config["AiSettings:Model"] ?? "gpt-3.5-turbo",
                    messages = new object[]
                    {
                        new { role = "system", content = "You are an expense categorization assistant. Respond with ONLY the category name from the provided list." },
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.3,
                    max_tokens = 10
                };

                var content = new StringContent( JsonSerializer.Serialize(request),  Encoding.UTF8,      "application/json");

                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config["AiSettings:OpenAiApiKey"]}");
              
                _logger.LogDebug("Sending request to OpenAI API for category prediction");

                var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenAI request failed: {Status}: {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
                    return "Other";
                }

                var json = await response.Content.ReadAsStringAsync();
                var predictedCategory = ExtractCategory(json);

                _logger.LogInformation("Category prediction successful. Input: {Description}, Predicted: {Category}",
                    description, predictedCategory);

                return predictedCategory;
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

                var openAiResponse = JsonSerializer.Deserialize<OpenAiChatResponse>(doc.RootElement.GetRawText());

                if (openAiResponse?.Choices?.Count == 0)
                {
                    _logger.LogWarning("No choices returned from OpenAI API");
                    return "Other";
                }
                var text = openAiResponse.Choices[0].Message.Content;
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Empty content in OpenAI response");
                    return "Other";
                }

                var normalized = text.Trim();

                if (!AllowedCategories.Contains(normalized))
                {
                    _logger.LogDebug("Predicted category not in allowed list. Normalized: {Normalized}", normalized);
                    return "Other";
                }

                return normalized;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse OpenAI response");
                return "Other";
            }
        }
    }

}
