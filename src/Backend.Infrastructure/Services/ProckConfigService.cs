using Backend.Core.Domain.Entities;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class ProckConfigService : IProckConfigService
{
    private readonly ProckDbContext _context;

    public ProckConfigService(ProckDbContext context)
    {
        _context = context;
    }

    public async Task<ProckConfig?> GetConfigAsync()
    {
        return await _context.ProckConfig.SingleOrDefaultAsync();
    }

    public async Task<ProckConfig> UpdateUpstreamUrlAsync(string? upstreamUrl)
    {
        var config = await _context.ProckConfig.SingleOrDefaultAsync();
        
        if (config == null)
        {
            config = new ProckConfig
            {
                Id = Guid.NewGuid(),
                UpstreamUrl = upstreamUrl
            };
            _context.ProckConfig.Add(config);
        }
        else
        {
            config.UpstreamUrl = upstreamUrl;
        }

        await _context.SaveChangesAsync();
        return config;
    }
}
