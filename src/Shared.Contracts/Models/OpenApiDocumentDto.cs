namespace Shared.Contracts.Models;

/// <summary>
/// Represents a summary of an OpenAPI document.
/// </summary>
public class OpenApiDocumentDto
{
    /// <summary>Unique identifier for the document.</summary>
    public Guid DocumentId { get; set; }
    /// <summary>Title of the API.</summary>
    public string? Title { get; set; }
    /// <summary>Version of the API.</summary>
    public string? Version { get; set; }
    /// <summary>Description of the API.</summary>
    public string? Description { get; set; }
    /// <summary>OpenAPI version string (e.g., 3.0.0).</summary>
    public string? OpenApiVersion { get; set; }
    /// <summary>Base path for the API (e.g., /v1).</summary>
    public string? BasePath { get; set; }
    /// <summary>Host for the API (e.g., api.example.com).</summary>
    public string? Host { get; set; }
    /// <summary>Supported schemes (e.g., HTTP, HTTPS).</summary>
    public List<string> Schemes { get; set; } = new();
    /// <summary>Supported content types for requests (e.g., application/json).</summary>
    public List<string> Consumes { get; set; } = new();
    /// <summary>Supported content types for responses (e.g., application/json).</summary>
    public List<string> Produces { get; set; } = new();
    /// <summary>Creation timestamp (UTC).</summary>
    public DateTime CreatedAt { get; set; }
    /// <summary>Last update timestamp (UTC).</summary>
    public DateTime UpdatedAt { get; set; }
    /// <summary>Whether the document is active and available for use.</summary>
    public bool IsActive { get; set; }
    /// <summary>List of API paths defined in the document.</summary>
    public List<OpenApiPathDto> Paths { get; set; } = new();
}

/// <summary>
/// DTO for creating a new OpenAPI document.
/// </summary>
public class CreateOpenApiDocumentDto
{
    /// <summary>Title of the API.</summary>
    public string? Title { get; set; }
    /// <summary>Version of the API.</summary>
    public string? Version { get; set; }
    /// <summary>Description of the API.</summary>
    public string? Description { get; set; }
    /// <summary>OpenAPI document in JSON format.</summary>
    public string? OpenApiJson { get; set; }
}

/// <summary>
/// DTO for updating an existing OpenAPI document.
/// </summary>
public class UpdateOpenApiDocumentDto
{
    /// <summary>Unique identifier for the document.</summary>
    public Guid DocumentId { get; set; }
    /// <summary>Title of the API.</summary>
    public string? Title { get; set; }
    /// <summary>Version of the API.</summary>
    public string? Version { get; set; }
    /// <summary>Description of the API.</summary>
    public string? Description { get; set; }
    /// <summary>OpenAPI document in JSON format.</summary>
    public string? OpenApiJson { get; set; }
    /// <summary>Indicates if the document is active.</summary>
    public bool? IsActive { get; set; }
}

/// <summary>
/// Detailed DTO for an OpenAPI document, including tags, servers, and original JSON.
/// </summary>
public class OpenApiDocumentDetailDto : OpenApiDocumentDto
{
    /// <summary>List of tags defined in the API.</summary>
    public List<string> Tags { get; set; } = new();
    /// <summary>List of servers defined in the API.</summary>
    public List<OpenApiServerDto> Servers { get; set; } = new();
    /// <summary>The original OpenAPI JSON string.</summary>
    public string? OriginalJson { get; set; }
}

/// <summary>
/// DTO representing a single API path.
/// </summary>
public class OpenApiPathDto
{
    /// <summary>Path of the API (e.g., /users).</summary>
    public string? Path { get; set; }
    /// <summary>Summary of the API path.</summary>
    public string? Summary { get; set; }
    /// <summary>Description of the API path.</summary>
    public string? Description { get; set; }
    /// <summary>List of operations for this path (future use).</summary>
    //public List<OpenApiOperationDto> Operations { get; set; } = new();
}

/// <summary>
/// DTO representing an operation (endpoint) in an API path.
/// </summary>
public class OpenApiOperationDto
{
    /// <summary>HTTP method (e.g., GET, POST).</summary>
    public string? HttpMethod { get; set; }
    /// <summary>Unique identifier for the operation.</summary>
    public string? OperationId { get; set; }
    /// <summary>Summary of the operation.</summary>
    public string? Summary { get; set; }
    /// <summary>Description of the operation.</summary>
    public string? Description { get; set; }
    /// <summary>Tags associated with the operation.</summary>
    public List<string> Tags { get; set; } = new();
    /// <summary>Indicates if the operation is deprecated.</summary>
    public bool? Deprecated { get; set; }
}

/// <summary>
/// DTO representing a server entry in the OpenAPI document.
/// </summary>
public class OpenApiServerDto
{
    /// <summary>URL of the server (e.g., https://api.example.com).</summary>
    public string? Url { get; set; }
    /// <summary>Description of the server.</summary>
    public string? Description { get; set; }
}
