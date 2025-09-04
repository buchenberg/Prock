namespace Shared.Contracts.Responses;

public class ConfigResponse
{
    public string? UpstreamUrl { get; set; }
    public string? Host { get; set; }
    public string? Port { get; set; }
    public string? ConnectionString { get; set; }
}
