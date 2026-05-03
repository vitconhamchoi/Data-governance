using Microsoft.AspNetCore.Mvc;
using QueryGateway.Models;
using QueryGateway.Services;

namespace QueryGateway.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueryController : ControllerBase
    {
        private readonly ITrinoService _trinoService;
        private readonly IPolicyService _policyService;
        private readonly IMaskingService _maskingService;
        private readonly ILogger<QueryController> _logger;

        public QueryController(
            ITrinoService trinoService,
            IPolicyService policyService,
            IMaskingService maskingService,
            ILogger<QueryController> logger)
        {
            _trinoService = trinoService;
            _policyService = policyService;
            _maskingService = maskingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<QueryResult>> ExecuteQuery([FromBody] QueryRequest request)
        {
            try
            {
                // Extract dataset from SQL (simple parsing - assumes FROM tablename)
                var dataset = request.Dataset ?? ExtractDatasetFromSql(request.Sql);

                if (string.IsNullOrEmpty(dataset))
                {
                    return BadRequest(new QueryResult
                    {
                        Success = false,
                        Error = "Could not determine dataset from query"
                    });
                }

                // Execute query via Trino
                var result = await _trinoService.ExecuteQuery(request.Sql);

                if (!result.Success)
                {
                    return Ok(result);
                }

                // Fetch applicable policies
                var policies = await _policyService.GetPoliciesForDatasetAndRole(dataset, request.Role);

                // Apply masking based on policies
                if (policies.Any())
                {
                    foreach (var row in result.Data)
                    {
                        foreach (var policy in policies)
                        {
                            if (row.ContainsKey(policy.Column))
                            {
                                row[policy.Column] = _maskingService.ApplyMasking(
                                    row[policy.Column],
                                    policy.Column,
                                    policy.Rule);

                                if (!result.AppliedPolicies.Contains($"{policy.Column}:{policy.Rule}"))
                                {
                                    result.AppliedPolicies.Add($"{policy.Column}:{policy.Rule}");
                                }
                            }
                        }
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing query");
                return StatusCode(500, new QueryResult
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        private string ExtractDatasetFromSql(string sql)
        {
            // Simple regex to extract table name from FROM clause
            var match = System.Text.RegularExpressions.Regex.Match(
                sql,
                @"FROM\s+(\w+)",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}
