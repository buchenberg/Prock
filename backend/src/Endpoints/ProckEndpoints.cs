using System.Diagnostics;
using System.Net;
using backend.Data.Dto;
using backend.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;

namespace backend.Endpoints;

public static class ProckEndpoints
{
    private static readonly string[] HttpMethods = [
        "GET", "PUT", "POST", "PATCH", "DELETE"
        ];

    public static void RegisterProckEndpoints(this WebApplication app)
    {

        app.MapGet("/prock/api/mock-routes", async Task<Ok<List<MockRouteDto>>> (IMockRouteRepository repo) =>
        {
            var routes = await repo.GetAllRoutesAsync();
            return TypedResults.Ok(routes);
        });


        app.MapGet("/prock/api/mock-routes/{routeId}",
            async Task<Results<Ok<MockRouteDto>, NotFound>> (Guid routeId, IMockRouteRepository repo) =>
            {
                var route = await repo.GetRouteByIdAsync(routeId);
                return route != null ? TypedResults.Ok(route) : TypedResults.NotFound();
            });

        app.MapGet("/prock/api/http-status-codes",
            () =>
            {
                var names = Enum.GetNames(typeof(HttpStatusCode));
                var result = new Dictionary<int, string>();
                foreach (var name in names)
                {
                    var key = (int)Enum.Parse(typeof(HttpStatusCode), name);
                    result.TryAdd(key, $"{key} {name}");
                }
                return Results.Ok(result);
            }
        );

        app.MapGet("/prock/api/http-content-types",
           () =>
           {
               return Results.Ok(ContentTypes.ToArray);
           }
       );

        app.MapPost("/prock/api/mock-routes",
            async Task<Results<Created<MockRouteDto>, BadRequest<string>>> (MockRouteDto route, IMockRouteRepository repo, CancellationToken cancellationToken) =>
        {
            route.Method = (route.Method ?? string.Empty).ToUpper();

            if (HttpMethods.All(x => x != route.Method))
            {
                return TypedResults.BadRequest($"{route.Method} is not a valid HTTP method");
            }

            app.Logger.LogInformation("Adding {Path} ...", route.Path);
            var result = await repo.CreateRouteAsync(route);
            app.Logger.LogInformation("Saved {Path} as {Id}", result.Path, result.RouteId);

            return TypedResults.Created($"/prock/api/mock-routes/{result.RouteId}", result);
        });

        app.MapPut("/prock/api/mock-routes/{routeId}/disable-route",
            async Task<Results<Ok<MockRouteDto>, NotFound<Guid>>> (Guid routeId, IMockRouteRepository repo, CancellationToken cancellationToken) =>
        {
            var result = await repo.SetRouteEnabledAsync(routeId, false);

            if (result == null)
            {
                return TypedResults.NotFound(routeId);
            }

            app.Logger.LogInformation("Disabled route {Id}", result.RouteId);
            return TypedResults.Ok(result);
        });

        app.MapPut("/prock/api/mock-routes/{routeId}/enable-route",
            async Task<Results<Ok<MockRouteDto>, NotFound<Guid>>> (Guid routeId, IMockRouteRepository repo, CancellationToken cancellationToken) =>
            {
                var result = await repo.SetRouteEnabledAsync(routeId, true);

                if (result == null)
                {
                    return TypedResults.NotFound(routeId);
                }

                app.Logger.LogInformation("Enabled route {Id}", result.RouteId);
                return TypedResults.Ok(result);
            });

        app.MapPut("/prock/api/mock-routes", async Task<Results<Ok<MockRouteDto>, BadRequest<string>>> (MockRouteDto route, IMockRouteRepository repo, CancellationToken cancellationToken) =>
        {

            if (HttpMethods.All(x => x != route.Method))
            {
                return TypedResults.BadRequest($"{route.Method} is not a valid HTTP method");
            }


            app.Logger.LogInformation("Updating {Path} ...", route.Path);
            var result = await repo.UpdateRouteAsync(route);
            
            if (result == null)
            {
                return TypedResults.BadRequest($"Route {route.RouteId} not found");
            }

            app.Logger.LogInformation("Updated route {Id}", result.RouteId);

            return TypedResults.Ok(result);
        });

        app.MapDelete("/prock/api/mock-routes/{routeId}", async Task<Results<Ok, NotFound>> (Guid routeId, IMockRouteRepository repo) =>
        {
            var success = await repo.DeleteRouteAsync(routeId);
            if (!success)
            {
                return TypedResults.NotFound();
            }

            app.Logger.LogInformation("Removed route {Id}", routeId);

            return TypedResults.Ok();
        });

        app.MapDelete("/prock/api/mock-routes", async Task<Ok<int>> (IMockRouteRepository repo) =>
        {
            var deletedCount = await repo.DeleteAllRoutesAsync();
            app.Logger.LogInformation("Deleted all {Count} mock routes", deletedCount);
            return TypedResults.Ok(deletedCount);
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