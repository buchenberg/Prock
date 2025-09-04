using System.ComponentModel.DataAnnotations;

namespace Backend.Core.Domain.Entities.MariaDb;

public class ProckConfig
{
    [Key]
    public int Id { get; set; }
    
    public string? Host { get; set; }
    
    public string? Port { get; set; }
    
    public string? UpstreamUrl { get; set; }
    
    public string? MongoDbUri { get; set; }
    
    public string? DbName { get; set; }
}


