using System.Text;
using System.Text.Json;

namespace AIClassifier.Services
{
    public interface IDataHubService
    {
        Task<bool> TagColumnAsync(string datasetName, string columnName, string tag);
    }

    public class DataHubService : IDataHubService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DataHubService> _logger;

        public DataHubService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<DataHubService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> TagColumnAsync(string datasetName, string columnName, string tag)
        {
            try
            {
                var gmsUrl = _configuration["DataHub:GmsUrl"] ?? "http://datahub-gms:8080";
                var client = _httpClientFactory.CreateClient("datahub");

                // DataHub URNs
                var datasetUrn = $"urn:li:dataset:(urn:li:dataPlatform:postgresql,public.{datasetName},PROD)";
                var tagUrn = $"urn:li:tag:{tag.Replace(".", "_")}";

                // Ensure tag entity exists in DataHub
                await EnsureTagAsync(client, gmsUrl, tagUrn, tag);

                // Attach tag to schema field via editableSchemaMetadata aspect
                var payload = new
                {
                    proposal = new
                    {
                        entityType = "dataset",
                        entityUrn = datasetUrn,
                        aspectName = "editableSchemaMetadata",
                        changeType = "UPSERT",
                        aspect = new
                        {
                            __type = "EditableSchemaMetadata",
                            editableSchemaFieldInfo = new[]
                            {
                                new
                                {
                                    fieldPath = columnName,
                                    globalTags = new
                                    {
                                        tags = new[] { new { tag = tagUrn } }
                                    }
                                }
                            }
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{gmsUrl}/aspects?action=ingestProposal", httpContent);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("DataHub ingestProposal failed [{Status}]: {Body}", response.StatusCode, body);
                    return false;
                }

                _logger.LogInformation("Tagged {Dataset}.{Column} with {Tag} in DataHub", datasetName, columnName, tag);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing tag to DataHub for {Dataset}.{Column}", datasetName, columnName);
                return false;
            }
        }

        private async Task EnsureTagAsync(HttpClient client, string gmsUrl, string tagUrn, string tagName)
        {
            try
            {
                var payload = new
                {
                    proposal = new
                    {
                        entityType = "tag",
                        entityUrn = tagUrn,
                        aspectName = "tagProperties",
                        changeType = "UPSERT",
                        aspect = new
                        {
                            __type = "TagProperties",
                            name = tagName,
                            description = $"Auto-classified PII/sensitive data tag: {tagName}"
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                await client.PostAsync($"{gmsUrl}/aspects?action=ingestProposal", httpContent);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not ensure tag {Tag} exists in DataHub", tagName);
            }
        }
    }
}
