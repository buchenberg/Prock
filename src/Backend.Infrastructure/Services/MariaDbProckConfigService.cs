using Backend.Core.Domain.Entities.MariaDb;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Services;

public class ProckConfigService : IProckConfigService
{
    private readonly MariaDbContext _context;

    public ProckConfigService(MariaDbContext context)
    {
        _context = context;
    }

    public async Task<ProckConfig?> GetConfigAsync()
    {
        return await _context.ProckConfigs.FirstOrDefaultAsync();
    }

    public async Task<ProckConfig> CreateOrUpdateConfigAsync(ProckConfig config)
    {
        var existingConfig = await GetConfigAsync();
        
        if (existingConfig != null)
        {
            // Update existing config
            existingConfig.Host = config.Host;
            existingConfig.Port = config.Port;
            existingConfig.UpstreamUrl = config.UpstreamUrl;
            existingConfig.MongoDbUri = config.MongoDbUri;
            existingConfig.DbName = config.DbName;
            
            _context.ProckConfigs.Update(existingConfig);
            await _context.SaveChangesAsync();
            return existingConfig;
        }
        else
        {
            // Create new config
            _context.ProckConfigs.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }
    }

    public async Task<ProckConfig> UpdateUpstreamUrlAsync(string? upstreamUrl)
    {
        var config = await GetConfigAsync();
        
        if (config != null)
        {
            config.UpstreamUrl = upstreamUrl;
            _context.ProckConfigs.Update(config);
            await _context.SaveChangesAsync();
            return config;
        }
        else
        {
            // Create new config with just the upstream URL
            var newConfig = new ProckConfig
            {
                UpstreamUrl = upstreamUrl
            };
            _context.ProckConfigs.Add(newConfig);
            await _context.SaveChangesAsync();
            return newConfig;
        }
    }
}
