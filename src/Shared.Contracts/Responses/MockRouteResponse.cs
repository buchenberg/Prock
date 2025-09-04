namespace Shared.Contracts.Responses;

public class MockRouteResponse
{
    public int Id { get; set; }
    public string RouteId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int HttpStatusCode { get; set; }
    public string? Mock { get; set; }
    public bool Enabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
