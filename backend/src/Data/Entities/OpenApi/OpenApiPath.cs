using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Data.Entities;

public class OpenApiPath
{
    [BsonElement("path")]
    public string? Path { get; set; }

    // [BsonElement("operations")]
    // public List<ObjectId> Operations { get; set; } = [];
    
    // [BsonElement("parameters")]
    // public List<ObjectId> Parameters { get; set; } = [];

    [BsonElement("summary")]
    public string? Summary { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
}

public class OpenApiOperation
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("pathId")]
    public ObjectId PathId { get; set; }
    [BsonElement("httpMethod")]
    public string? HttpMethod { get; set; }
    
    [BsonElement("operationId")]
    public string? OperationId { get; set; }
    
    [BsonElement("summary")]
    public string? Summary { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("tags")]
    public List<string>? Tags { get; set; }
    
    // [BsonElement("parameters")]
    // public List<OpenApiParameter>? Parameters { get; set; }
    
    // [BsonElement("requestBody")]
    // public OpenApiRequestBody? RequestBody { get; set; }
    
    // [BsonElement("responses")]
    // public List<OpenApiResponse>? Responses { get; set; }
    
    // [BsonElement("security")]
    // public List<OpenApiSecurityRequirement>? Security { get; set; }
    
    [BsonElement("deprecated")]
    public bool? Deprecated { get; set; }
    
    [BsonElement("externalDocs")]
    public OpenApiExternalDocumentation? ExternalDocs { get; set; }
}

public class OpenApiParameter
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("pathId")]
    public ObjectId PathId { get; set; }
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("in")]
    public string? In { get; set; } // query, header, path, cookie
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("required")]
    public bool? Required { get; set; }
    
    [BsonElement("deprecated")]
    public bool? Deprecated { get; set; }
    
    [BsonElement("schema")]
    public OpenApiSchema? Schema { get; set; }
    
    [BsonElement("example")]
    public object? Example { get; set; }
    
    [BsonElement("examples")]
    public Dictionary<string, OpenApiExample>? Examples { get; set; }
}

public class OpenApiRequestBody
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("oasDocumentId")]
    public Guid OasDocumentId { get; set; }
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("content")]
    public Dictionary<string, OpenApiMediaType>? Content { get; set; }
    
    [BsonElement("required")]
    public bool? Required { get; set; }
}

public class OpenApiResponse
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("oasDocumentId")]
    public Guid OasDocumentId { get; set; }
    [BsonElement("statusCode")]
    public string? StatusCode { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("headers")]
    public Dictionary<string, OpenApiHeader>? Headers { get; set; }
    
    [BsonElement("content")]
    public Dictionary<string, OpenApiMediaType>? Content { get; set; }
}

public class OpenApiMediaType
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("oasDocumentId")]
    public Guid OasDocumentId { get; set; }
    [BsonElement("schema")]
    public OpenApiSchema? Schema { get; set; }
    
    [BsonElement("example")]
    public object? Example { get; set; }
    
    [BsonElement("examples")]
    public Dictionary<string, OpenApiExample>? Examples { get; set; }
}

public class OpenApiHeader
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("oasDocumentId")]
    public Guid OasDocumentId { get; set; }
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("required")]
    public bool? Required { get; set; }
    
    [BsonElement("deprecated")]
    public bool? Deprecated { get; set; }
    
    [BsonElement("schema")]
    public OpenApiSchema? Schema { get; set; }
}

public class OpenApiExample
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("oasDocumentId")]
    public Guid OasDocumentId { get; set; }
    [BsonElement("summary")]
    public string? Summary { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("value")]
    public object? Value { get; set; }
    
    [BsonElement("externalValue")]
    public string? ExternalValue { get; set; }
}

public class OpenApiSecurityRequirement
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("oasDocumentId")]
    public Guid OasDocumentId { get; set; }
    [BsonElement("schemeName")]
    public string? SchemeName { get; set; }
    
    [BsonElement("scopes")]
    public List<string>? Scopes { get; set; }
}
