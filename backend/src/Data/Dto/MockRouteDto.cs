namespace Prock.Backend.Data.Dto;

public class MockRouteDto
{
    public Guid RouteId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int HttpStatusCode { get; set; }
    public object? Mock { get; set; }
    public bool Enabled { get; set; }
}