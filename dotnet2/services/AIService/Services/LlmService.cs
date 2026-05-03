using System.Text;
using System.Text.Json;

namespace AIService.Services
{
    public interface ILlmService
    {
        Task<string> CompleteAsync(string systemPrompt, string userMessage, int maxTokens = 1024);
    }

    public class LlmService : ILlmService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LlmService> _logger;

        public LlmService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<LlmService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> CompleteAsync(string systemPrompt, string userMessage, int maxTokens = 1024)
        {
            var apiKey = _configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");
            var baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
            var model = _configuration["OpenAI:Model"] ?? "gpt-4o-mini";

            var client = _httpClientFactory.CreateClient("llm");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var body = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.1,
                max_tokens = maxTokens
            };

            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{baseUrl}/chat/completions", content);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("LLM API error {Status}: {Body}", response.StatusCode, raw);
                throw new HttpRequestException($"LLM API {response.StatusCode}: {raw}");
            }

            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? string.Empty;
        }
    }
}
