namespace DrugUserPreventionUI.Models.Common
{
    /// <summary>
    /// Simple API response wrapper for deserialization
    /// </summary>
    public class SimpleApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
} 