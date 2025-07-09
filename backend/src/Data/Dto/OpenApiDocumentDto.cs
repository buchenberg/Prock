namespace backend.Data.Dto;

public class OpenApiDocumentDto
{
    public Guid DocumentId { get; set; }
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? OpenApiVersion { get; set; }
    public string? BasePath { get; set; }
    public string? Host { get; set; }
    public List<string> Schemes { get; set; } = [];
    public List<string> Consumes { get; set; } = [];
    public List<string> Produces { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<OpenApiPathDto> Paths { get; set; } = [];
}

public class CreateOpenApiDocumentDto
{
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? OpenApiJson { get; set; }
}

public class UpdateOpenApiDocumentDto
{
    public Guid DocumentId { get; set; }
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Description { get; set; }
    public string? OpenApiJson { get; set; }
    public bool? IsActive { get; set; }
}

public class OpenApiDocumentDetailDto : OpenApiDocumentDto
{
    
    public List<string> Tags { get; set; } = [];
    public List<OpenApiServerDto> Servers { get; set; } = [];
    public string? OriginalJson { get; set; }
}

public class OpenApiPathDto
{
    public string? Path { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    //public List<OpenApiOperationDto> Operations { get; set; } = [];
}

public class OpenApiOperationDto
{
    public string? HttpMethod { get; set; }
    public string? OperationId { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = [];
    public bool? Deprecated { get; set; }
}

public class OpenApiServerDto
{
    public string? Url { get; set; }
    public string? Description { get; set; }
}
