namespace QueryGateway.Models
{
    public class QueryRequest
    {
        public required string Sql { get; set; }
        public required string Role { get; set; }
        public string? Dataset { get; set; }
    }

    public class QueryResult
    {
        public List<Dictionary<string, object?>> Data { get; set; } = new();
        public List<string> Columns { get; set; } = new();
        public int RowCount { get; set; }
        public bool Success { get; set; }
        public string? Error { get; set; }
        public List<string> AppliedPolicies { get; set; } = new();
    }
}
