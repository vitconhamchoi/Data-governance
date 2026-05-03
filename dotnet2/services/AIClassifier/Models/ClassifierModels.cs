namespace AIClassifier.Models
{
    public class ClassifyRequest
    {
        public required string TableName { get; set; }
        public required string ColumnName { get; set; }
        public List<string> SampleValues { get; set; } = new();
    }

    public class ClassifyResult
    {
        public string ColumnName { get; set; } = string.Empty;
        /// <summary>PII.email | PII.phone | PII.name | sensitive | none</summary>
        public string Type { get; set; } = string.Empty;
        public double Confidence { get; set; }
        /// <summary>regex | llm</summary>
        public string DetectionMethod { get; set; } = string.Empty;
    }

    public class ScanTableRequest
    {
        public required string TableName { get; set; }
        public int SampleSize { get; set; } = 5;
    }

    public class ScanTableResult
    {
        public string TableName { get; set; } = string.Empty;
        public List<ClassifyResult> Classifications { get; set; } = new();
        public List<string> TagsPushedToDataHub { get; set; } = new();
    }

    public class ColumnTag
    {
        public int Id { get; set; }
        public required string TableName { get; set; }
        public required string ColumnName { get; set; }
        public required string Tag { get; set; }
        public double Confidence { get; set; }
        public string DetectionMethod { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
