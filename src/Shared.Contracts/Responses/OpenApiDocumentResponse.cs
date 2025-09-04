namespace Shared.Contracts.Responses;

public class OpenApiDocumentResponse
{
    public string DocumentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? OpenApiVersion { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public int PathCount { get; set; }
}
