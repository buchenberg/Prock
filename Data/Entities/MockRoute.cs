using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DrunkenMaster.Net;

public class MockRoute
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("routeId")]
    public Guid RouteId { get; set; }
    [BsonElement("method")]
    public string Method { get; set; }
    [BsonElement("path")]
    public string Path { get; set; }
    [BsonElement("mock")]
    public string Mock { get; set; }
}