namespace DrunkenMaster.Net.Data.Dto;

public class MockRouteDto
{
    public Guid RouteId { get; set; }

    public string Method { get; set; }
    public string Path { get; set; }
    public dynamic? Mock { get; set; }
}