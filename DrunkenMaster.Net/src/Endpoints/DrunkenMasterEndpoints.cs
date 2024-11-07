using System.Diagnostics;
using System.Text.Json;
using DrunkenMaster.Net.Data;
using DrunkenMaster.Net.Data.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DrunkenMaster.Net.Endpoints;

public static class DrunkenMasterEndpoints
{
    public static void RegisterDrunkenMasterEndpoints(this WebApplication app, DrunkenMasterDbContext db)
    {
        var connectionString = app.Configuration.GetSection("DrunkenMaster").GetSection("MongoDbUri").Value ??
                               "mongodb://localhost:27017/";
        var upstreamUrl = app.Configuration.GetSection("DrunkenMaster").GetSection("UpstreamUrl").Value ??
                          "https://example.com";
        var host = app.Configuration.GetSection("DrunkenMaster").GetSection("Host").Value ?? "http://localhost";
        var port = app.Configuration.GetSection("DrunkenMaster").GetSection("Port").Value ?? "5000";

        app.MapGet("/drunken-master/api/config", () => Results.Ok(new { connectionString, upstreamUrl, host, port }));

        app.MapGet("/drunken-master/api/mock-routes", async Task<Results<Ok<List<MockRouteDto>>, Ok>> () =>
            await db.MockRoutes.ToListAsync() is List<MockRoute> response
                ? TypedResults.Ok(response.Select(x => new MockRouteDto()
                    {
                        RouteId = x.RouteId,
                        Method = x.Method,
                        Path = x.Path,
                        Mock = JsonSerializer.Deserialize<dynamic>(x.Mock)
                    }).ToList()
                )
                : TypedResults.Ok());

        app.MapGet("/drunken-master/api/mock-routes/{routeId}",
            async Task<Results<Ok<MockRouteDto>, NotFound>> (Guid routeId) =>
                await db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeId) is MockRoute response
                    ? TypedResults.Ok(new MockRouteDto()
                    {
                        RouteId = response.RouteId,
                        Method = response.Method,
                        Path = response.Path,
                        Mock = JsonSerializer.Deserialize<dynamic>(response.Mock)
                    })
                    : TypedResults.NotFound());

        app.MapPost("/drunken-master/api/mock-routes",
            async (MockRouteDto route, CancellationToken cancellationToken) =>
            {
                var methods = new[]
                {
                    "Get", "Put", "Post", "Patch", "Delete"
                };
                if (methods.All(x => x != route.Method))
                {
                    return Results.BadRequest($"{route.Method} is not a valid HTTP method");
                }

                var result = new MockRoute
                {
                    RouteId = Guid.NewGuid(),
                    Method = route.Method,
                    Path = route.Path,
                    Mock = JsonSerializer.Serialize(route.Mock)
                };
                app.Logger.LogInformation("Adding {Path} ...", route.Path);
                db.MockRoutes.Add(result);
                await db.SaveChangesAsync(cancellationToken);
                app.Logger.LogInformation("Saved {Path} as {Id}", result.Path, result.RouteId);
                var dto = new MockRouteDto
                {
                    RouteId = result.RouteId,
                    Method = result.Method,
                    Path = result.Path,
                    Mock = JsonSerializer.Deserialize<dynamic>(route.Mock)
                };

                return TypedResults.Created($"/drunken-master/api/mock-routes/{result.RouteId}", dto);
            });

        app.MapPut("/drunken-master/api/mock-routes",
            async Task<Results<Ok<MockRouteDto>, BadRequest<string>>> (MockRouteDto route,
                CancellationToken cancellationToken) =>
            {
                var methods = new[]
                {
                    "Get", "Put", "Post", "Patch", "Delete"
                };
                if (methods.All(x => x != route.Method))
                {
                    return TypedResults.BadRequest($"{route.Method} is not a valid HTTP method");
                }


                app.Logger.LogInformation("Updating {Path} ...", route.Path);
                var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == route.RouteId);
                if (persistedRoute == null)
                {
                    return TypedResults.BadRequest($"Route {route.RouteId} not found");
                }

                persistedRoute.Path = route.Path;
                persistedRoute.Method = route.Method;
                persistedRoute.Mock = JsonSerializer.Serialize(route.Mock);
                await db.SaveChangesAsync(cancellationToken);
                app.Logger.LogInformation("Updated route {Id}", persistedRoute.RouteId);

                var dto = new MockRouteDto
                {
                    RouteId = persistedRoute.RouteId,
                    Method = persistedRoute.Method,
                    Path = persistedRoute.Path,
                    Mock = JsonSerializer.Deserialize<dynamic>(route.Mock)
                };

                return TypedResults.Ok(dto);
            });

        app.MapDelete("/drunken-master/api/mock-routes/{routeId}", async Task<Results<Ok, NotFound>> (Guid routeId) =>
        {
            var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId);
            if (persistedRoute == null)
            {
                return TypedResults.NotFound();
            }

            db.MockRoutes.Remove(persistedRoute);

            await db.SaveChangesAsync();
            app.Logger.LogInformation("Removed route {Id}", persistedRoute.RouteId);

            return TypedResults.Ok();
        });

        app.MapPost("/drunken-master/api/restart", async Task<int> (CancellationToken cancellationToken) =>
        {
            app.Logger.LogInformation("Restart called...");

            var currentProcess = Path.GetFullPath(Process.GetCurrentProcess().MainModule?.FileName ?? "Program.cs");
            app.Lifetime.StopApplication();
            Process.Start(currentProcess);
            return await Task.FromResult(0);
        });

        app.MapPost("/drunken-master/api/kill",
            (CancellationToken cancellationToken) => app.Lifetime.StopApplication());
    }
}