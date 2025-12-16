using backend.Data.Entities;

namespace backend.Repositories;

public interface IProckConfigRepository
{
    Task<ProckConfig?> GetConfigAsync();
    Task<ProckConfig> UpdateUpstreamUrlAsync(string upstreamUrl);
}
