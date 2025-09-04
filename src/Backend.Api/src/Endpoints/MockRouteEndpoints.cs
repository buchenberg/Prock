using System.Net;
using Backend.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Shared.Contracts.Models;
using Shared.Contracts.Requests;
using Shared.Contracts.Responses;

namespace Backend.Api.Endpoints;

public static class MockRouteEndpoints
{
    public static void RegisterMockRouteEndpoints(this WebApplication app)
    {
        var routes = app.MapGroup(ApiRoutes.MockRoutes.Base)
            .WithTags("Mock Routes")
            .WithOpenApi();

        // GET /prock/api/mock-routes
        routes.MapGet("", GetAllMockRoutes)
            .WithName("GetAllMockRoutes")
            .WithSummary("Get all mock routes")
            .Produces<IEnumerable<MockRouteResponse>>();

        // GET /prock/api/mock-routes/{id}
        routes.MapGet("{id}", GetMockRouteById)
            .WithName("GetMockRouteById")
            .WithSummary("Get a mock route by ID")
            .Produces<MockRouteResponse>()
            .Produces(404);

        // POST /prock/api/mock-routes
        routes.MapPost("", CreateMockRoute)
            .WithName("CreateMockRoute")
            .WithSummary("Create a new mock route")
            .Produces<MockRouteResponse>(201)
            .ProducesValidationProblem();

        // PUT /prock/api/mock-routes/{id}
        routes.MapPut("{id}", UpdateMockRoute)
            .WithName("UpdateMockRoute")
            .WithSummary("Update an existing mock route")
            .Produces<MockRouteResponse>()
            .Produces(404)
            .ProducesValidationProblem();

        // DELETE /prock/api/mock-routes/{id}
        routes.MapDelete("{id}", DeleteMockRoute)
            .WithName("DeleteMockRoute")
            .WithSummary("Delete a mock route")
            .Produces(204)
            .Produces(404);

        // PUT /prock/api/mock-routes/{id}/enable
        routes.MapPut("{id}/enable", EnableMockRoute)
            .WithName("EnableMockRoute")
            .WithSummary("Enable a mock route")
            .Produces<MockRouteResponse>()
            .Produces(404);

        // PUT /prock/api/mock-routes/{id}/disable
        routes.MapPut("{id}/disable", DisableMockRoute)
            .WithName("DisableMockRoute")
            .WithSummary("Disable a mock route")
            .Produces<MockRouteResponse>()
            .Produces(404);

        // Utility endpoints
        app.MapGet("/prock/api/http-status-codes", GetHttpStatusCodes)
            .WithTags("Utilities")
            .WithName("GetHttpStatusCodes")
            .WithSummary("Get all HTTP status codes")
            .Produces<Dictionary<int, string>>();

        app.MapGet("/prock/api/http-content-types", GetHttpContentTypes)
            .WithTags("Utilities")
            .WithName("GetHttpContentTypes")
            .WithSummary("Get all HTTP content types")
            .Produces<string[]>();
    }

    private static async Task<Results<Ok<IEnumerable<MockRouteResponse>>, ProblemHttpResult>> GetAllMockRoutes(
        IMockRouteService mockRouteService)
    {
        try
        {
            var mockRoutes = await mockRouteService.GetAllAsync();
            var responses = mockRoutes.Select(MapToResponse);
            return TypedResults.Ok(responses);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve mock routes: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<MockRouteResponse>, NotFound, ProblemHttpResult>> GetMockRouteById(
        string id, IMockRouteService mockRouteService)
    {
        try
        {
            var mockRoute = await mockRouteService.GetByIdAsync(id);
            return mockRoute != null 
                ? TypedResults.Ok(MapToResponse(mockRoute))
                : TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to retrieve mock route: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Created<MockRouteResponse>, ValidationProblem, ProblemHttpResult>> CreateMockRoute(
        CreateMockRouteRequest request, IMockRouteService mockRouteService, ILogger<Program> logger)
    {
        try
        {
            var mockRoute = MapToEntity(request);
            var created = await mockRouteService.CreateAsync(mockRoute);
            
            logger.LogInformation("Created mock route {RouteId} for {Method} {Path}", 
                created.RouteId, created.Method, created.Path);
            
            var response = MapToResponse(created);
            return TypedResults.Created($"/prock/api/mock-routes/{created.RouteId}", response);
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to create mock route: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<MockRouteResponse>, NotFound, ValidationProblem, ProblemHttpResult>> UpdateMockRoute(
        string id, UpdateMockRouteRequest request, IMockRouteService mockRouteService, ILogger<Program> logger)
    {
        try
        {
            var mockRoute = MapToEntity(request);
            var updated = await mockRouteService.UpdateAsync(id, mockRoute);
            
            logger.LogInformation("Updated mock route {RouteId} for {Method} {Path}", 
                updated.RouteId, updated.Method, updated.Path);
            
            return TypedResults.Ok(MapToResponse(updated));
        }
        catch (InvalidOperationException)
        {
            return TypedResults.NotFound();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to update mock route: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<NoContent, NotFound, ProblemHttpResult>> DeleteMockRoute(
        string id, IMockRouteService mockRouteService, ILogger<Program> logger)
    {
        try
        {
            var deleted = await mockRouteService.DeleteAsync(id);
            if (!deleted)
            {
                return TypedResults.NotFound();
            }
            
            logger.LogInformation("Deleted mock route {RouteId}", id);
            return TypedResults.NoContent();
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to delete mock route: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<MockRouteResponse>, NotFound, ProblemHttpResult>> EnableMockRoute(
        string id, IMockRouteService mockRouteService, ILogger<Program> logger)
    {
        try
        {
            var enabled = await mockRouteService.EnableAsync(id);
            if (!enabled)
            {
                return TypedResults.NotFound();
            }
            
            var mockRoute = await mockRouteService.GetByIdAsync(id);
            logger.LogInformation("Enabled mock route {RouteId}", id);
            
            return TypedResults.Ok(MapToResponse(mockRoute!));
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to enable mock route: {ex.Message}", statusCode: 500);
        }
    }

    private static async Task<Results<Ok<MockRouteResponse>, NotFound, ProblemHttpResult>> DisableMockRoute(
        string id, IMockRouteService mockRouteService, ILogger<Program> logger)
    {
        try
        {
            var disabled = await mockRouteService.DisableAsync(id);
            if (!disabled)
            {
                return TypedResults.NotFound();
            }
            
            var mockRoute = await mockRouteService.GetByIdAsync(id);
            logger.LogInformation("Disabled mock route {RouteId}", id);
            
            return TypedResults.Ok(MapToResponse(mockRoute!));
        }
        catch (Exception ex)
        {
            return TypedResults.Problem($"Failed to disable mock route: {ex.Message}", statusCode: 500);
        }
    }

    private static Ok<Dictionary<int, string>> GetHttpStatusCodes()
    {
        var names = Enum.GetNames(typeof(HttpStatusCode));
        var result = new Dictionary<int, string>();
        
        foreach (var name in names)
        {
            var key = (int)Enum.Parse(typeof(HttpStatusCode), name);
            result.TryAdd(key, $"{key} {name}");
        }
        
        return TypedResults.Ok(result);
    }

    private static Ok<string[]> GetHttpContentTypes()
    {
        return TypedResults.Ok(ContentTypes.ToArray);
    }

    // Mapping helpers
    private static MockRouteResponse MapToResponse(Backend.Core.Domain.Entities.MockRoute entity)
    {
        return new MockRouteResponse
        {
            RouteId = entity.RouteId.ToString(),
            Method = entity.Method ?? string.Empty,
            Path = entity.Path ?? string.Empty,
            HttpStatusCode = entity.HttpStatusCode,
            Mock = entity.Mock,
            Enabled = entity.Enabled,
            CreatedAt = DateTime.UtcNow, // TODO: Add timestamps to entity
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Backend.Core.Domain.Entities.MockRoute MapToEntity(CreateMockRouteRequest request)
    {
        return new Backend.Core.Domain.Entities.MockRoute
        {
            Method = request.Method,
            Path = request.Path,
            HttpStatusCode = request.HttpStatusCode,
            Mock = request.Mock,
            Enabled = request.Enabled
        };
    }

    private static Backend.Core.Domain.Entities.MockRoute MapToEntity(UpdateMockRouteRequest request)
    {
        return new Backend.Core.Domain.Entities.MockRoute
        {
            Method = request.Method,
            Path = request.Path,
            HttpStatusCode = request.HttpStatusCode,
            Mock = request.Mock,
            Enabled = request.Enabled
        };
    }
}
