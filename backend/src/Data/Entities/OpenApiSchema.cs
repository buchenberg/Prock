using MongoDB.Bson.Serialization.Attributes;

namespace backend.Data.Entities;

public class OpenApiSchema
{
    [BsonElement("type")]
    public string? Type { get; set; }
    
    [BsonElement("format")]
    public string? Format { get; set; }
    
    [BsonElement("title")]
    public string? Title { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("default")]
    public object? Default { get; set; }
    
    [BsonElement("example")]
    public object? Example { get; set; }
    
    [BsonElement("enum")]
    public List<object>? Enum { get; set; }
    
    [BsonElement("multipleOf")]
    public decimal? MultipleOf { get; set; }
    
    [BsonElement("maximum")]
    public decimal? Maximum { get; set; }
    
    [BsonElement("exclusiveMaximum")]
    public bool? ExclusiveMaximum { get; set; }
    
    [BsonElement("minimum")]
    public decimal? Minimum { get; set; }
    
    [BsonElement("exclusiveMinimum")]
    public bool? ExclusiveMinimum { get; set; }
    
    [BsonElement("maxLength")]
    public int? MaxLength { get; set; }
    
    [BsonElement("minLength")]
    public int? MinLength { get; set; }
    
    [BsonElement("pattern")]
    public string? Pattern { get; set; }
    
    [BsonElement("maxItems")]
    public int? MaxItems { get; set; }
    
    [BsonElement("minItems")]
    public int? MinItems { get; set; }
    
    [BsonElement("uniqueItems")]
    public bool? UniqueItems { get; set; }
    
    [BsonElement("maxProperties")]
    public int? MaxProperties { get; set; }
    
    [BsonElement("minProperties")]
    public int? MinProperties { get; set; }
    
    [BsonElement("required")]
    public List<string>? Required { get; set; }
    
    [BsonElement("items")]
    public OpenApiSchema? Items { get; set; }
    
    [BsonElement("allOf")]
    public List<OpenApiSchema>? AllOf { get; set; }
    
    [BsonElement("oneOf")]
    public List<OpenApiSchema>? OneOf { get; set; }
    
    [BsonElement("anyOf")]
    public List<OpenApiSchema>? AnyOf { get; set; }
    
    [BsonElement("not")]
    public OpenApiSchema? Not { get; set; }
    
    [BsonElement("properties")]
    public Dictionary<string, OpenApiSchema>? Properties { get; set; }
    
    [BsonElement("additionalProperties")]
    public OpenApiSchema? AdditionalProperties { get; set; }
    
    [BsonElement("discriminator")]
    public OpenApiDiscriminator? Discriminator { get; set; }
    
    [BsonElement("readOnly")]
    public bool? ReadOnly { get; set; }
    
    [BsonElement("writeOnly")]
    public bool? WriteOnly { get; set; }
    
    [BsonElement("xml")]
    public OpenApiXml? Xml { get; set; }
    
    [BsonElement("externalDocs")]
    public OpenApiExternalDocumentation? ExternalDocs { get; set; }
    
    [BsonElement("deprecated")]
    public bool? Deprecated { get; set; }
    
    [BsonElement("nullable")]
    public bool? Nullable { get; set; }
    
    [BsonElement("reference")]
    public string? Reference { get; set; } // $ref
}

public class OpenApiDiscriminator
{
    [BsonElement("propertyName")]
    public string? PropertyName { get; set; }
    
    [BsonElement("mapping")]
    public Dictionary<string, string>? Mapping { get; set; }
}

public class OpenApiXml
{
    [BsonElement("name")]
    public string? Name { get; set; }
    
    [BsonElement("namespace")]
    public string? Namespace { get; set; }
    
    [BsonElement("prefix")]
    public string? Prefix { get; set; }
    
    [BsonElement("attribute")]
    public bool? Attribute { get; set; }
    
    [BsonElement("wrapped")]
    public bool? Wrapped { get; set; }
}
