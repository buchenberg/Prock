using System.ComponentModel.DataAnnotations;
using Shared.Contracts.Models;

namespace Shared.Contracts.Requests;

public class UpdateMockRouteRequest
{
    [Required]
    [ValidHttpMethod]
    public string Method { get; set; } = string.Empty;

    [Required]
    [ValidPath]
    [StringLength(2000)]
    public string Path { get; set; } = string.Empty;

    [Range(100, 599)]
    public int HttpStatusCode { get; set; } = 200;

    [ValidJson]
    public string? Mock { get; set; }

    public bool Enabled { get; set; } = true;
}
