namespace Shared.Contracts.Models;

public class MockRouteDto
{
    public Guid RouteId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int HttpStatusCode { get; set; }
    public dynamic? Mock { get; set; }
    public bool Enabled { get; set; }
}