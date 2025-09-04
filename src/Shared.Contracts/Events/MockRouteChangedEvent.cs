namespace Shared.Contracts.Events;

public class MockRouteChangedEvent
{
    public string RouteId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "created", "updated", "deleted", "enabled", "disabled"
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
