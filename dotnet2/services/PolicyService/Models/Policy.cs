namespace PolicyService.Models
{
    public class Policy
    {
        public int Id { get; set; }
        public required string Dataset { get; set; }
        public required string Column { get; set; }
        public required string Rule { get; set; }  // "mask" or "deny"
        public required string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
