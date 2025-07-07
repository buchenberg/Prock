using backend.Data.Entities;

namespace backend.Data.Dto;

public class ProckConfigDto(ProckConfig config)
{
    public string? UpstreamUrl { get; set; } = config.UpstreamUrl;

}