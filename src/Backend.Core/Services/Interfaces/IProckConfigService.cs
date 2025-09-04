using Backend.Core.Domain.Entities;

namespace Backend.Core.Services.Interfaces;

public interface IProckConfigService
{
    Task<ProckConfig?> GetConfigAsync();
    Task<ProckConfig> UpdateUpstreamUrlAsync(string? upstreamUrl);
}
