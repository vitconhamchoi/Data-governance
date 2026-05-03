using QueryGateway.Models;

namespace QueryGateway.Services
{
    public interface IPolicyService
    {
        Task<List<Policy>> GetPoliciesForDatasetAndRole(string dataset, string role);
    }

    public class PolicyService : IPolicyService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PolicyService> _logger;

        public PolicyService(IHttpClientFactory httpClientFactory, ILogger<PolicyService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<List<Policy>> GetPoliciesForDatasetAndRole(string dataset, string role)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PolicyService");
                var response = await client.GetAsync($"/api/policies/dataset/{dataset}/role/{role}");

                if (response.IsSuccessStatusCode)
                {
                    var policies = await response.Content.ReadFromJsonAsync<List<Policy>>();
                    return policies ?? new List<Policy>();
                }
                else
                {
                    _logger.LogWarning($"Failed to fetch policies: {response.StatusCode}");
                    return new List<Policy>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching policies");
                return new List<Policy>();
            }
        }
    }
}
