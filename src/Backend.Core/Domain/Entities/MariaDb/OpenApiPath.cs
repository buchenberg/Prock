using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Domain.Entities.MariaDb;

public class OpenApiPath
{
    [Key]
    public int Id { get; set; }
    
    public int OpenApiSpecificationId { get; set; }
    
    public required string Path { get; set; }
    
    public required string Method { get; set; }
    
    public string? Summary { get; set; }
    
    public string? Description { get; set; }
    
    public string? OperationId { get; set; }
    
    public string? RequestBody { get; set; } // JSON
    
    public string? Responses { get; set; } // JSON
    
    public string? Parameters { get; set; } // JSON
    
    // Navigation property
    public OpenApiSpecification? OpenApiSpecification { get; set; }
}


