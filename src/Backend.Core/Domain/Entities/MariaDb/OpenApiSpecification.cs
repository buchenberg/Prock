using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Domain.Entities.MariaDb;

public class OpenApiSpecification
{
    [Key]
    public int Id { get; set; }
    
    public required string Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? Version { get; set; }
    
    public string? OpenApiVersion { get; set; }
    
    public required string Content { get; set; } // JSON content
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
}


