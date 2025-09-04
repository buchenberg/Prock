using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Contracts.Models;
using Shared.Contracts.Requests;
using Shared.Contracts.Responses;

namespace Backend.Api.Endpoints;

public static class ConfigurationEndpoints
{
    public static void RegisterConfigurationEndpoints(this WebApplication app)
    {
        var routes = app.MapGroup(ApiRoutes.Config.Base)
            .WithTags("Configuration")
            .WithOpenApi();

        // GET /prock/api/config
        routes.MapGet("", GetConfiguration)
            .WithName("GetConfiguration")
            .WithSummary("Get application configuration")
            .Produces<ConfigResponse>();

        // PUT /prock/api/config/upstream-url
        routes.MapPut("upstream-url", UpdateUpstreamUrl)
            .WithName("UpdateUpstreamUrl")
            .WithSummary("Update the upstream URL configuration")
            .Produces<ConfigResponse>()
            .ProducesValidationProblem();
    }

    private static async Task<Results<Ok<ConfigResponse>, ProblemHttpResult>> GetConfiguration(
        IProckConfigService configService, IConfiguration configuration)
    {
        try
        {
            var connectionString = configuration.GetSection("Prock:MongoDbUri").Value ?? "mongodb://localhost:27017/";
            var host = configuration.GetSection("Prock:Host").Value ?? "http://localhost";
            var port = configuration.GetSection("Prock:Port").Value ?? "5001";

            var config = await configService.GetConfigAsync();
            var upstreamUrl = config?.UpstreamUrl ?? "nope";

            var response = new ConfigResponse
            {
                UpstreamUrl = upstreamUrl,
                Host = host,
                Port = port,
                ConnectionString = connectionString
            };

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve configuration: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<ConfigResponse>, ValidationProblem, ProblemHttpResult>> UpdateUpstreamUrl(
        UpdateUpstreamUrlRequest request, IProckConfigService configService, IConfiguration configuration, ILogger<Program> logger)
    {
        try
        {
            var updatedConfig = await configService.UpdateUpstreamUrlAsync(request.UpstreamUrl);
            
            logger.LogInformation("Updated upstream URL to: {UpstreamUrl}", request.UpstreamUrl ?? "null");

            // Return the full configuration response
            var connectionString = configuration.GetSection("Prock:MongoDbUri").Value ?? "mongodb://localhost:27017/";
            var host = configuration.GetSection("Prock:Host").Value ?? "http://localhost";
            var port = configuration.GetSection("Prock:Port").Value ?? "5001";

            var response = new ConfigResponse
            {
                UpstreamUrl = updatedConfig.UpstreamUrl,
                Host = host,
                Port = port,
                ConnectionString = connectionString
            };

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to update upstream URL: {ex.Message}", statusCode: 500);
        }
    }
}
