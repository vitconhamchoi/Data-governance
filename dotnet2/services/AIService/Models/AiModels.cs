namespace AIService.Models
{
    // ── NL2SQL ────────────────────────────────────────────────────────────────

    public class NL2SqlRequest
    {
        public required string Question { get; set; }
        public string Role { get; set; } = "analyst";
        /// <summary>Optional Trino catalog qualifier, e.g. "postgresql"</summary>
        public string Catalog { get; set; } = "postgresql";
    }

    public class NL2SqlResult
    {
        public string GeneratedSql { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<Dictionary<string, object?>> Data { get; set; } = new();
        public List<string> Columns { get; set; } = new();
        public int RowCount { get; set; }
        public List<string> AppliedPolicies { get; set; } = new();
    }

    // ── Copilot ───────────────────────────────────────────────────────────────

    public class CopilotRequest
    {
        public required string Question { get; set; }
    }

    public class CopilotResponse
    {
        public required string Answer { get; set; }
        public List<string> Sources { get; set; } = new();
    }

    // ── Semantic Search ───────────────────────────────────────────────────────

    public class SemanticSearchRequest
    {
        public required string Query { get; set; }
        public int TopK { get; set; } = 5;
    }

    public class SemanticSearchResult
    {
        /// <summary>table | column</summary>
        public string EntityType { get; set; } = string.Empty;
        public string EntityName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Score { get; set; }
    }

    public class IndexMetadataResponse
    {
        public int EntitiesIndexed { get; set; }
        public List<string> Indexed { get; set; } = new();
    }

    // ── Policy Recommendation ─────────────────────────────────────────────────

    public class PolicyRecommendation
    {
        public int Id { get; set; }
        public required string TableName { get; set; }
        public required string ColumnName { get; set; }
        public required string Tag { get; set; }
        /// <summary>MASK | DENY</summary>
        public required string Rule { get; set; }
        public required string Reason { get; set; }
        /// <summary>pending | approved | rejected</summary>
        public string Status { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewedBy { get; set; }
    }

    public class RecommendRequest
    {
        public required string TableName { get; set; }
        public required string ColumnName { get; set; }
        public required string Tag { get; set; }
    }

    public class ReviewRequest
    {
        /// <summary>approved | rejected</summary>
        public required string Decision { get; set; }
        public string ReviewedBy { get; set; } = "human";
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    public class MetadataEmbedding
    {
        public int Id { get; set; }
        public required string EntityType { get; set; }
        public required string EntityName { get; set; }
        public required string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
