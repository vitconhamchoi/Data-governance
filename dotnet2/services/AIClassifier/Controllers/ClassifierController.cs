using AIClassifier.Data;
using AIClassifier.Models;
using AIClassifier.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AIClassifier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClassifierController : ControllerBase
    {
        private readonly ClassifierDbContext _context;
        private readonly IPiiDetectorService _piiDetector;
        private readonly ILlmService _llmService;
        private readonly IDataHubService _dataHubService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ClassifierController> _logger;

        public ClassifierController(
            ClassifierDbContext context,
            IPiiDetectorService piiDetector,
            ILlmService llmService,
            IDataHubService dataHubService,
            IConfiguration configuration,
            ILogger<ClassifierController> logger)
        {
            _context = context;
            _piiDetector = piiDetector;
            _llmService = llmService;
            _dataHubService = dataHubService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>Classify a single column for PII using regex + LLM fallback.</summary>
        [HttpPost("classify")]
        public async Task<ActionResult<ClassifyResult>> ClassifyColumn([FromBody] ClassifyRequest request)
        {
            // Fast path: regex
            var regexResult = _piiDetector.DetectWithRegex(request.ColumnName, request.SampleValues);
            if (regexResult != null && regexResult.Confidence >= 0.7)
            {
                await PersistTagAsync(request.TableName, regexResult);
                return Ok(regexResult);
            }

            // LLM fallback
            var llmResult = await ClassifyWithLlmAsync(request);
            await PersistTagAsync(request.TableName, llmResult);
            return Ok(llmResult);
        }

        /// <summary>Scan all columns in a table, classify PII and push tags to DataHub.</summary>
        [HttpPost("scan")]
        public async Task<ActionResult<ScanTableResult>> ScanTable([FromBody] ScanTableRequest request)
        {
            var result = new ScanTableResult { TableName = request.TableName };

            var columns = await GetTableColumnsAsync(request.TableName);
            if (columns.Count == 0)
                return NotFound(new { error = $"Table '{request.TableName}' not found or has no columns" });

            foreach (var column in columns)
            {
                var sampleValues = await GetSampleValuesAsync(request.TableName, column, request.SampleSize);

                var regexResult = _piiDetector.DetectWithRegex(column, sampleValues);
                ClassifyResult classification;

                if (regexResult != null && regexResult.Confidence >= 0.7)
                {
                    classification = regexResult;
                }
                else
                {
                    classification = await ClassifyWithLlmAsync(new ClassifyRequest
                    {
                        TableName = request.TableName,
                        ColumnName = column,
                        SampleValues = sampleValues
                    });
                }

                result.Classifications.Add(classification);

                if (classification.Type != "none" && classification.Confidence >= 0.6)
                {
                    await PersistTagAsync(request.TableName, classification);
                    bool pushed = await _dataHubService.TagColumnAsync(request.TableName, column, classification.Type);
                    if (pushed)
                        result.TagsPushedToDataHub.Add($"{column}:{classification.Type}");
                }
            }

            return Ok(result);
        }

        /// <summary>Get all stored classification tags for a table.</summary>
        [HttpGet("tags/{tableName}")]
        public async Task<ActionResult<IEnumerable<ColumnTag>>> GetTagsByTable(string tableName)
        {
            var tags = await _context.ColumnTags
                .Where(t => t.TableName == tableName)
                .ToListAsync();
            return Ok(tags);
        }

        /// <summary>Get all stored classification tags.</summary>
        [HttpGet("tags")]
        public async Task<ActionResult<IEnumerable<ColumnTag>>> GetAllTags()
        {
            return Ok(await _context.ColumnTags.ToListAsync());
        }

        // ── Private helpers ─────────────────────────────────────────────────────

        private async Task<ClassifyResult> ClassifyWithLlmAsync(ClassifyRequest request)
        {
            const string systemPrompt =
                "You are a PII classifier. Analyze column name and sample values to detect personal or sensitive data. " +
                "Return ONLY valid JSON: {\"type\": \"<label>\", \"confidence\": <0.0-1.0>}. " +
                "Labels: PII.email, PII.phone, PII.name, sensitive (SSN/credit-card/password/DOB/salary), none.";

            var userMessage =
                $"Table: {request.TableName}\n" +
                $"Column: {request.ColumnName}\n" +
                $"Sample values: {string.Join(", ", request.SampleValues.Take(5))}\n\n" +
                "Classify this column and return JSON only.";

            try
            {
                var response = await _llmService.CompleteAsync(systemPrompt, userMessage);

                // Extract JSON from response (LLM may wrap it in code fences)
                var jsonStart = response.IndexOf('{');
                var jsonEnd = response.LastIndexOf('}');
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonStr = response[jsonStart..(jsonEnd + 1)];
                    using var doc = JsonDocument.Parse(jsonStr);
                    return new ClassifyResult
                    {
                        ColumnName = request.ColumnName,
                        Type = doc.RootElement.GetProperty("type").GetString() ?? "none",
                        Confidence = doc.RootElement.GetProperty("confidence").GetDouble(),
                        DetectionMethod = "llm"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LLM classification failed for {Table}.{Column}", request.TableName, request.ColumnName);
            }

            return new ClassifyResult
            {
                ColumnName = request.ColumnName,
                Type = "none",
                Confidence = 0.0,
                DetectionMethod = "llm_error"
            };
        }

        private async Task PersistTagAsync(string tableName, ClassifyResult result)
        {
            if (result.Type is "none" or "llm_error") return;

            var existing = await _context.ColumnTags
                .FirstOrDefaultAsync(t => t.TableName == tableName && t.ColumnName == result.ColumnName);

            if (existing != null)
            {
                existing.Tag = result.Type;
                existing.Confidence = result.Confidence;
                existing.DetectionMethod = result.DetectionMethod;
            }
            else
            {
                _context.ColumnTags.Add(new ColumnTag
                {
                    TableName = tableName,
                    ColumnName = result.ColumnName,
                    Tag = result.Type,
                    Confidence = result.Confidence,
                    DetectionMethod = result.DetectionMethod
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task<List<string>> GetTableColumnsAsync(string tableName)
        {
            var columns = new List<string>();
            try
            {
                var cs = _configuration.GetConnectionString("DefaultConnection")!;
                await using var conn = new Npgsql.NpgsqlConnection(cs);
                await conn.OpenAsync();

                await using var cmd = new Npgsql.NpgsqlCommand(
                    "SELECT column_name FROM information_schema.columns " +
                    "WHERE table_schema = 'public' AND table_name = @t " +
                    "ORDER BY ordinal_position", conn);
                cmd.Parameters.AddWithValue("t", tableName);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    columns.Add(reader.GetString(0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching columns for table {Table}", tableName);
            }
            return columns;
        }

        private async Task<List<string>> GetSampleValuesAsync(string tableName, string columnName, int limit)
        {
            var values = new List<string>();
            // Column and table names are validated against information_schema above — safe to use as identifiers
            if (!IsValidIdentifier(tableName) || !IsValidIdentifier(columnName))
                return values;

            try
            {
                var cs = _configuration.GetConnectionString("DefaultConnection")!;
                await using var conn = new Npgsql.NpgsqlConnection(cs);
                await conn.OpenAsync();

                var sql = $"SELECT \"{columnName}\"::text FROM \"{tableName}\" WHERE \"{columnName}\" IS NOT NULL LIMIT {limit}";
                await using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    values.Add(reader.GetString(0));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching sample values for {Table}.{Column}", tableName, columnName);
            }
            return values;
        }

        private static bool IsValidIdentifier(string name) =>
            !string.IsNullOrWhiteSpace(name) &&
            System.Text.RegularExpressions.Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
    }
}
