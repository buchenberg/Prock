using System.ComponentModel.DataAnnotations;

namespace Prock.Backend.src.Data.MariaDb;

public class MockRoute
{
    [Key]
    public int Id { get; set; }
    public required string RouteId { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int HttpStatusCode { get; set; }
    public string? Mock { get; set; }
    public bool Enabled { get; set; }
}