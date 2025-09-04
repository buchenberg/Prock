using System.ComponentModel.DataAnnotations;

namespace Shared.Contracts.Requests;

public class CreateOpenApiDocumentRequest
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Version { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string OriginalJson { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
