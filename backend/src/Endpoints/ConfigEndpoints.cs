using backend.Data.Dto;
using backend.Repositories;

namespace backend.Endpoints;

public static class ConfigEndpoints
{
    public static void RegisterConfigEndpoints(this WebApplication app)
    {
        var connectionString = app.Configuration.GetSection("Prock").GetSection("MongoDbUri").Value ??
                               "mongodb://localhost:27017/";

        var defaultUpstreamUrl = app.Configuration.GetSection("Prock").GetSection("UpstreamUrl").Value ?? "https://example.com";

        app.MapGet("/prock/api/config", async (IProckConfigRepository repo) =>
        {
            var host = app.Configuration.GetSection("Prock").GetSection("Host").Value
            ?? "http://localhost";
            var port = app.Configuration.GetSection("Prock").GetSection("Port").Value
            ?? "5001";
            var config = await repo.GetConfigAsync();
            if (config == null)
            {
                return TypedResults.Ok(new
                {
                    connectionString,
                    upstreamUrl = defaultUpstreamUrl,
                    host,
                    port

                });
            }
            var upstreamUrl = config.UpstreamUrl ?? defaultUpstreamUrl;
            return TypedResults.Ok(new { connectionString, upstreamUrl, host, port });
        });
        app.MapPut("/prock/api/config/upstream-url", async (ProckConfigDto update, IProckConfigRepository repo, CancellationToken cancellationToken) =>
        {
            var config = await repo.UpdateUpstreamUrlAsync(update.UpstreamUrl);
            return TypedResults.Ok(config);
        });

    }
}
