using System.Text;
using System.Text.Json;
using AIService.Models;

namespace AIService.Services
{
    public interface ITrinoQueryService
    {
        Task<NL2SqlResult> ExecuteAsync(string sql);
    }

    public class TrinoQueryService : ITrinoQueryService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TrinoQueryService> _logger;

        public TrinoQueryService(IConfiguration configuration, ILogger<TrinoQueryService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<NL2SqlResult> ExecuteAsync(string sql)
        {
            var result = new NL2SqlResult { GeneratedSql = sql };

            try
            {
                var host = _configuration["TrinoConnection:Host"] ?? "trino";
                var port = _configuration["TrinoConnection:Port"] ?? "8080";

                using var client = new HttpClient();
                client.BaseAddress = new Uri($"http://{host}:{port}");
                client.DefaultRequestHeaders.Add("X-Trino-User", "ai-service");

                var content = new StringContent(sql, Encoding.UTF8, "text/plain");
                var response = await client.PostAsync("/v1/statement", content);

                if (!response.IsSuccessStatusCode)
                {
                    result.Error = $"Trino rejected query: {response.StatusCode}";
                    return result;
                }

                var body = await response.Content.ReadAsStringAsync();
                var trinoResp = JsonSerializer.Deserialize<TrinoResponse>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Follow pagination
                while (!string.IsNullOrEmpty(trinoResp?.NextUri))
                {
                    response = await client.GetAsync(trinoResp.NextUri);
                    body = await response.Content.ReadAsStringAsync();
                    trinoResp = JsonSerializer.Deserialize<TrinoResponse>(body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }

                if (trinoResp?.Columns != null)
                    result.Columns = trinoResp.Columns.Select(c => c.Name).ToList();

                if (trinoResp?.Data != null)
                {
                    foreach (var row in trinoResp.Data)
                    {
                        var dict = new Dictionary<string, object?>();
                        for (int i = 0; i < result.Columns.Count && i < row.Count; i++)
                            dict[result.Columns[i]] = row[i];
                        result.Data.Add(dict);
                    }
                }

                result.RowCount = result.Data.Count;
                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Trino execution error");
                result.Error = ex.Message;
            }

            return result;
        }

        private sealed class TrinoResponse
        {
            public string? NextUri { get; set; }
            public List<TrinoColumn>? Columns { get; set; }
            public List<List<object?>>? Data { get; set; }
        }

        private sealed class TrinoColumn
        {
            public string Name { get; set; } = string.Empty;
        }
    }
}
