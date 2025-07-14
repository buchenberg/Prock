using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Prock.Backend.Data.Entities;

public class ProckConfig
{
    [BsonId]
    public Guid Id { get; set; }
    [BsonElement("upstreamUrl")]
    public string? UpstreamUrl { get; set; }
  
}