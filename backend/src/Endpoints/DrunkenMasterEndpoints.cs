using System.Diagnostics;
using System.Net;
using System.Text.Json;
using backend.Data;
using backend.Data.Dto;
using backend.Data.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class ProckEndpoints
{
    private static readonly string[] HttpMethods = ["Get", "Put", "Post", "Patch", "Delete"];

    public static void RegisterProckEndpoints(this WebApplication app)
    {
        var connectionString = app.Configuration.GetSection("Prock").GetSection("MongoDbUri").Value ??
                               "mongodb://localhost:27017/";
        var upstreamUrl = app.Configuration.GetSection("Prock").GetSection("UpstreamUrl").Value ??
                          "https://example.com";
        var host = app.Configuration.GetSection("Prock").GetSection("Host").Value ?? "http://localhost";
        var port = app.Configuration.GetSection("Prock").GetSection("Port").Value ?? "5001";

        app.MapGet("/prock/api/config", () => Results.Ok(new { connectionString, upstreamUrl, host, port }));

        app.MapGet("/prock/api/mock-routes", async Task<Results<Ok<List<MockRouteDto>>, Ok>> (ProckDbContext db) =>
            await db.MockRoutes.ToListAsync() is List<MockRoute> response
                ? TypedResults.Ok(response.Select(x => new MockRouteDto()
                {
                    RouteId = x.RouteId,
                    Method = x.Method,
                    Path = x.Path,
                    HttpStatusCode = x.HttpStatusCode,
                    Mock = JsonSerializer.Deserialize<dynamic>(x.Mock),
                    Enabled = x.Enabled
                }).ToList()
                )
                : TypedResults.Ok());

        app.MapGet("/prock/api/mock-routes/{routeId}",
            async Task<Results<Ok<MockRouteDto>, NotFound>> (Guid routeId, ProckDbContext db) =>
                await db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeId) is MockRoute response
                    ? TypedResults.Ok(new MockRouteDto()
                    {
                        RouteId = response.RouteId,
                        Method = response.Method,
                        Path = response.Path,
                        HttpStatusCode = response.HttpStatusCode,
                        Mock = JsonSerializer.Deserialize<dynamic>(response.Mock),
                        Enabled = response.Enabled
                    })
                    : TypedResults.NotFound());

        app.MapGet("/prock/api/http-status-codes",
            () =>
            {
                var names = Enum.GetNames(typeof(HttpStatusCode));
                var result = new Dictionary<int, string>();
                foreach (var name in names)
                {
                    var key = (int)Enum.Parse(typeof(HttpStatusCode), name);
                    if (result.ContainsKey(key)) continue;
                    result.Add(key, name);
                }
                return Results.Ok(result);
            }
        );

        app.MapPost("/prock/api/mock-routes",
            async (MockRouteDto route, ProckDbContext db, CancellationToken cancellationToken) =>
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
                HttpStatusCode = route.HttpStatusCode,
                Mock = JsonSerializer.Serialize(route.Mock),
                Enabled = true
            };

            app.Logger.LogInformation("Adding {Path} ...", route.Path);
            db.MockRoutes.Add(result);
            await db.SaveChangesAsync(cancellationToken);

            app.Logger.LogInformation("Saved {Path} as {Id}", result.Path, result.RouteId);

            route.RouteId = result.RouteId;

            return TypedResults.Created($"/prock/api/mock-routes/{result.RouteId}", route);
        });

        app.MapPut("/prock/api/mock-routes/{routeId}/disable-route",
            async Task<Results<Ok<MockRouteDto>, NotFound<Guid>>> (Guid routeId, ProckDbContext db, CancellationToken cancellationToken) =>
        {

            var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId);

            if (persistedRoute == null)
            {
                return TypedResults.NotFound(routeId);
            }

            persistedRoute.Enabled = false;

            await db.SaveChangesAsync(cancellationToken);

            app.Logger.LogInformation("Disabled route {Id}", persistedRoute.RouteId);

            var response = new MockRouteDto
            {
                RouteId = persistedRoute.RouteId,
                Method = persistedRoute.Method,
                Path = persistedRoute.Path,
                HttpStatusCode = persistedRoute.HttpStatusCode,
                Mock = JsonSerializer.Serialize(persistedRoute.Mock),
                Enabled = persistedRoute.Enabled
            };

            return TypedResults.Ok(response);
        });

        app.MapPut("/prock/api/mock-routes/{routeId}/enable-route",
            async Task<Results<Ok<MockRouteDto>, NotFound<Guid>>> (Guid routeId, ProckDbContext db, CancellationToken cancellationToken) =>
            {

                var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId);

                if (persistedRoute == null)
                {
                    return TypedResults.NotFound(routeId);
                }

                persistedRoute.Enabled = true;

                await db.SaveChangesAsync(cancellationToken);

                app.Logger.LogInformation("Disabled route {Id}", persistedRoute.RouteId);

                var response = new MockRouteDto
                {
                    RouteId = persistedRoute.RouteId,
                    Method = persistedRoute.Method,
                    Path = persistedRoute.Path,
                    HttpStatusCode = persistedRoute.HttpStatusCode,
                    Mock = JsonSerializer.Serialize(persistedRoute.Mock),
                    Enabled = persistedRoute.Enabled
                };

                return TypedResults.Ok(response);
            });

        app.MapPut("/prock/api/mock-routes", async Task<Results<Ok<MockRouteDto>, BadRequest<string>>> (MockRouteDto route, ProckDbContext db, CancellationToken cancellationToken) =>
        {

            if (HttpMethods.All(x => x != route.Method))
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
            persistedRoute.HttpStatusCode = route.HttpStatusCode;
            persistedRoute.Mock = JsonSerializer.Serialize(route.Mock);

            await db.SaveChangesAsync(cancellationToken);

            app.Logger.LogInformation("Updated route {Id}", persistedRoute.RouteId);

            return TypedResults.Ok(route);
        });

        app.MapDelete("/prock/api/mock-routes/{routeId}", async Task<Results<Ok, NotFound>> (Guid routeId, ProckDbContext db) =>
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

        app.MapPost("/prock/api/restart", async Task<int> (CancellationToken cancellationToken) =>
        {
            app.Logger.LogInformation("Restart called...");

            var currentProcess = Path.GetFullPath(Process.GetCurrentProcess().MainModule?.FileName ?? "Program.cs");
            app.Lifetime.StopApplication();
            Process.Start(currentProcess);
            return await Task.FromResult(0);
        });

        app.MapPost("/prock/api/kill",
            (CancellationToken cancellationToken) => app.Lifetime.StopApplication());
    }
}