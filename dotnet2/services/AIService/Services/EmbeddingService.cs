using System.Text;
using System.Text.Json;

namespace AIService.Services
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string text);
    }

    public class EmbeddingService : IEmbeddingService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmbeddingService> _logger;

        public EmbeddingService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<EmbeddingService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            var apiKey = _configuration["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");
            var baseUrl = _configuration["OpenAI:BaseUrl"] ?? "https://api.openai.com/v1";
            var embeddingModel = _configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";

            var client = _httpClientFactory.CreateClient("llm");
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var body = new { model = embeddingModel, input = text };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/embeddings", content);
            var raw = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Embedding API error {Status}: {Body}", response.StatusCode, raw);
                throw new HttpRequestException($"Embedding API {response.StatusCode}: {raw}");
            }

            using var doc = JsonDocument.Parse(raw);
            return doc.RootElement
                .GetProperty("data")[0]
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(e => e.GetSingle())
                .ToArray();
        }
    }
}
