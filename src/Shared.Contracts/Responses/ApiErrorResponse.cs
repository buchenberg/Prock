namespace Shared.Contracts.Responses;

public class ApiErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public string? Detail { get; set; }
    public int StatusCode { get; set; }
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
