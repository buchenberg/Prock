namespace Shared.Contracts.Events;

public class ProxyRequestEvent
{
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsMocked { get; set; }
    public int StatusCode { get; set; }
    public string? UpstreamUrl { get; set; }
}
