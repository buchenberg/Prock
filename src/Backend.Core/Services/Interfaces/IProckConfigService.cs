using Backend.Core.Domain.Entities.MariaDb;

namespace Backend.Core.Services.Interfaces;

public interface IProckConfigService
{
    Task<ProckConfig?> GetConfigAsync();
    Task<ProckConfig> CreateOrUpdateConfigAsync(ProckConfig config);
    Task<ProckConfig> UpdateUpstreamUrlAsync(string? upstreamUrl);
}
