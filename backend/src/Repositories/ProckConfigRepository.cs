using backend.Data;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Repositories;

public class ProckConfigRepository : IProckConfigRepository
{
    private readonly ProckDbContext _db;

    public ProckConfigRepository(ProckDbContext db)
    {
        _db = db;
    }

    public async Task<ProckConfig?> GetConfigAsync()
    {
        return await _db.ProckConfig.SingleOrDefaultAsync();
    }

    public async Task<ProckConfig> UpdateUpstreamUrlAsync(string upstreamUrl)
    {
        var config = await _db.ProckConfig.SingleOrDefaultAsync();
        if (config == null)
        {
            config = new ProckConfig
            {
                Id = Guid.NewGuid(),
                UpstreamUrl = upstreamUrl
            };
            _db.ProckConfig.Add(config);
        }
        else
        {
            config.UpstreamUrl = upstreamUrl;
        }

        await _db.SaveChangesAsync();
        return config;
    }
}
