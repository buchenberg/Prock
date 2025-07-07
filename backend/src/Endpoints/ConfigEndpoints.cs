using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace backend.Endpoints;

public static class ConfigEndpoints
{
    public static void RegisterConfigEndpoints(this WebApplication app)
    {
        var connectionString = app.Configuration.GetSection("Prock").GetSection("MongoDbUri").Value ??
                               "mongodb://localhost:27017/";

        app.MapGet("/prock/api/config", async (ProckDbContext db) =>
        {
            var host = app.Configuration.GetSection("Prock").GetSection("Host").Value
            ?? "http://localhost";
            var port = app.Configuration.GetSection("Prock").GetSection("Port").Value
            ?? "5001";
            var config = await db.GetProckConfigAsync();
            if (config == null)
            {
                return TypedResults.Ok(new
                {
                    connectionString,
                    upstreamUrl = "nope",
                    host,
                    port

                });
            }
            var upstreamUrl = config.UpstreamUrl ?? "unknown";
            return TypedResults.Ok(new { connectionString, upstreamUrl, host, port });
        });
        app.MapPut("/prock/api/config/upstream-url", async (ProckConfigDto update, ProckDbContext db, CancellationToken cancellationToken) =>
        {
            var config = await db.ProckConfig.SingleOrDefaultAsync(cancellationToken);
            if (config == null)
            {
                config = new ProckConfig()
                {
                    Id = Guid.NewGuid(),
                    UpstreamUrl = update.UpstreamUrl
                };
                db.ProckConfig.Add(config);
                await db.SaveChangesAsync(cancellationToken);
                return Results.Ok(config);
            }
            config.UpstreamUrl = update.UpstreamUrl;
            await db.SaveChangesAsync(cancellationToken);
            return TypedResults.Ok(config);
        });

    }
}