using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Data.Entities;

public class OpenApiDocument
{
    [BsonId]
    public ObjectId _id { get; set; }
    
    [BsonElement("documentId")]
    public Guid DocumentId { get; set; }
    
    [BsonElement("title")]
    public string? Title { get; set; }
    
    [BsonElement("version")]
    public string? Version { get; set; }
    
    [BsonElement("description")]
    public string? Description { get; set; }
    
    [BsonElement("openApiVersion")]
    public string? OpenApiVersion { get; set; }
    
    [BsonElement("basePath")]
    public string? BasePath { get; set; }
    
    [BsonElement("host")]
    public string? Host { get; set; }
    
    [BsonElement("schemes")]
    public List<string>? Schemes { get; set; }
    
    [BsonElement("consumes")]
    public List<string>? Consumes { get; set; }
    
    [BsonElement("produces")]
    public List<string>? Produces { get; set; }
    
    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; }
    
    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; }
    
    [BsonElement("isActive")]
    public bool IsActive { get; set; }
    
    [BsonElement("originalJson")]
    public string? OriginalJson { get; set; }
    
    // Store complex nested objects as raw BSON to avoid EF Core limitations
    [BsonElement("pathsData")]
    public BsonDocument? PathsData { get; set; }
    
    [BsonElement("componentsData")]
    public BsonDocument? ComponentsData { get; set; }
    
    [BsonElement("tagsData")]
    public BsonDocument? TagsData { get; set; }
    
    [BsonElement("serversData")]
    public BsonDocument? ServersData { get; set; }
    
    [BsonElement("externalDocsData")]
    public BsonDocument? ExternalDocsData { get; set; }
}
