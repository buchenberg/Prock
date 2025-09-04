using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Backend.Core.Domain.Entities;

public class ProckConfig
{
    [BsonId]
    public Guid Id { get; set; }
    [BsonElement("upstreamUrl")]
    public string? UpstreamUrl { get; set; }
  
}