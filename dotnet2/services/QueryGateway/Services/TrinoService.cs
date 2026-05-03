using System.Text;
using System.Text.Json;
using QueryGateway.Models;

namespace QueryGateway.Services
{
    public interface ITrinoService
    {
        Task<QueryResult> ExecuteQuery(string sql);
    }

    public class TrinoService : ITrinoService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TrinoService> _logger;
        private readonly HttpClient _httpClient;

        public TrinoService(IConfiguration configuration, ILogger<TrinoService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();

            var trinoHost = _configuration["TrinoConnection:Host"] ?? "trino";
            var trinoPort = _configuration["TrinoConnection:Port"] ?? "8080";
            _httpClient.BaseAddress = new Uri($"http://{trinoHost}:{trinoPort}");
            _httpClient.DefaultRequestHeaders.Add("X-Trino-User", "admin");
        }

        public async Task<QueryResult> ExecuteQuery(string sql)
        {
            var result = new QueryResult { Success = false };

            try
            {
                var content = new StringContent(sql, Encoding.UTF8, "text/plain");
                var response = await _httpClient.PostAsync("/v1/statement", content);

                if (!response.IsSuccessStatusCode)
                {
                    result.Error = $"Trino query failed: {response.StatusCode}";
                    return result;
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var trinoResponse = JsonSerializer.Deserialize<TrinoResponse>(responseBody);

                if (trinoResponse == null)
                {
                    result.Error = "Failed to parse Trino response";
                    return result;
                }

                // Follow next URI if data is paginated
                while (!string.IsNullOrEmpty(trinoResponse.NextUri))
                {
                    response = await _httpClient.GetAsync(trinoResponse.NextUri);
                    responseBody = await response.Content.ReadAsStringAsync();
                    trinoResponse = JsonSerializer.Deserialize<TrinoResponse>(responseBody);

                    if (trinoResponse == null)
                        break;
                }

                // Extract columns and data
                if (trinoResponse?.Columns != null)
                {
                    result.Columns = trinoResponse.Columns.Select(c => c.Name).ToList();
                }

                if (trinoResponse?.Data != null)
                {
                    foreach (var row in trinoResponse.Data)
                    {
                        var rowDict = new Dictionary<string, object?>();
                        for (int i = 0; i < result.Columns.Count && i < row.Count; i++)
                        {
                            rowDict[result.Columns[i]] = row[i];
                        }
                        result.Data.Add(rowDict);
                    }
                }

                result.RowCount = result.Data.Count;
                result.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Trino query");
                result.Error = ex.Message;
            }

            return result;
        }
    }

    public class TrinoResponse
    {
        public string? Id { get; set; }
        public string? NextUri { get; set; }
        public List<TrinoColumn>? Columns { get; set; }
        public List<List<object?>>? Data { get; set; }
    }

    public class TrinoColumn
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}
