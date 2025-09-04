using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Core.Domain.Entities.OpenApi;

public class OpenApiComponents
{
    [BsonElement("schemas")]
    public Dictionary<string, OpenApiSchema>? Schemas { get; set; }
    
    [BsonElement("responses")]
    public Dictionary<string, OpenApiResponse>? Responses { get; set; }
    
    [BsonElement("parameters")]
    public Dictionary<string, OpenApiParameter>? Parameters { get; set; }
    
    [BsonElement("examples")]
    public Dictionary<string, OpenApiExample>? Examples { get; set; }
    
    [BsonElement("requestBodies")]
    public Dictionary<string, OpenApiRequestBody>? RequestBodies { get; set; }
    
    [BsonElement("headers")]
    public Dictionary<string, OpenApiHeader>? Headers { get; set; }
    
    [BsonElement("securitySchemes")]
    public Dictionary<string, OpenApiSecurityScheme>? SecuritySchemes { get; set; }
    
    [BsonElement("links")]
    public Dictionary<string, OpenApiLink>? Links { get; set; }
    
    [BsonElement("callbacks")]
    public Dictionary<string, OpenApiCallback>? Callbacks { get; set; }
}

public class OpenApiSecurityScheme
{
    [BsonElement("type")]
    public string? Type { get; set; } // apiKey, http, oauth2, openIdConnect
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("in")]
    public string? In { get; set; } // query, header, cookie
    
    [BsonElement("scheme")]
    public string? Scheme { get; set; } // bearer, basic, etc.
    
    [BsonElement("bearerFormat")]
    public string? BearerFormat { get; set; }
    
    [BsonElement("flows")]
    public OpenApiOAuthFlows? Flows { get; set; }
    
    [BsonElement("openIdConnectUrl")]
    public string? OpenIdConnectUrl { get; set; }
}

public class OpenApiOAuthFlows
{
    [BsonElement("implicit")]
    public OpenApiOAuthFlow? Implicit { get; set; }
    
    [BsonElement("password")]
    public OpenApiOAuthFlow? Password { get; set; }
    
    [BsonElement("clientCredentials")]
    public OpenApiOAuthFlow? ClientCredentials { get; set; }
    
    [BsonElement("authorizationCode")]
    public OpenApiOAuthFlow? AuthorizationCode { get; set; }
}

public class OpenApiOAuthFlow
{
    [BsonElement("authorizationUrl")]
    public string? AuthorizationUrl { get; set; }
    
    [BsonElement("tokenUrl")]
    public string? TokenUrl { get; set; }
    
    [BsonElement("refreshUrl")]
    public string? RefreshUrl { get; set; }
    
    [BsonElement("scopes")]
    public Dictionary<string, string>? Scopes { get; set; }
}

public class OpenApiLink
{
    [BsonElement("operationRef")]
    public string? OperationRef { get; set; }
    
    [BsonElement("operationId")]
    public string? OperationId { get; set; }
    
    [BsonElement("parameters")]
    public Dictionary<string, object>? Parameters { get; set; }
    
    [BsonElement("requestBody")]
    public object? RequestBody { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("server")]
    public OpenApiServer? Server { get; set; }
}

public class OpenApiCallback
{
    [BsonElement("expression")]
    public string? Expression { get; set; }
    
    [BsonElement("pathItem")]
    public OpenApiPath? PathItem { get; set; }
}

public class OpenApiTag
{
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("externalDocs")]
    public OpenApiExternalDocumentation? ExternalDocs { get; set; }
}

public class OpenApiServer
{
    [BsonElement("url")]
    public string? Url { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("variables")]
    public Dictionary<string, OpenApiServerVariable>? Variables { get; set; }
}

public class OpenApiServerVariable
{
    [BsonElement("enum")]
    public List<string>? Enum { get; set; }
    
    [BsonElement("default")]
    public string? Default { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
}

public class OpenApiExternalDocumentation
{
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("url")]
    public string? Url { get; set; }
}
