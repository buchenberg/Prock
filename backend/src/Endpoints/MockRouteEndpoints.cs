using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Prock.Backend.Data.Dto;
using Prock.Backend.src.Data.MariaDb;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;


namespace Prock.Backend.Endpoints;

public static class MockRouteEndpoints
{
    private static readonly string[] HttpMethods = [
        "GET", "PUT", "POST", "PATCH", "DELETE"
        ];

    public static void RegisterMockRouteEndpoints(this WebApplication app)
    {

        app.MapGet("/prock/api/mock-routes", async Task<Results<Ok<List<MockRouteDto>>, Ok>> (MariaDbContext db) =>
        {
            var response = await db.MockRoutes.ToListAsync();
            if (response != null && response.Count > 0)
            {
                return TypedResults.Ok(response.Select(x => new MockRouteDto()
                {
                    RouteId = Guid.Parse(x.RouteId),
                    Method = x.Method,
                    Path = x.Path,
                    HttpStatusCode = x.HttpStatusCode,
                    Mock = x.Mock != null ? JsonSerializer.Deserialize<dynamic>(x.Mock) : null,
                    Enabled = x.Enabled
                }).ToList());
            }
            else
            {
                return TypedResults.Ok();
            }
        });


        app.MapGet("/prock/api/mock-routes/{routeId}",
            async Task<Results<Ok<MockRouteDto>, NotFound>> (Guid routeId, MariaDbContext db) =>
            {
                var response = await db.MockRoutes.SingleOrDefaultAsync(x => x.RouteId == routeId.ToString());
                if (response != null)
                {
                    return TypedResults.Ok(new MockRouteDto()
                    {
                        RouteId = Guid.Parse(response.RouteId),
                        Method = response.Method,
                        Path = response.Path,
                        HttpStatusCode = response.HttpStatusCode,
                        Mock = response.Mock != null ? JsonSerializer.Deserialize<dynamic>(response.Mock) : null,
                        Enabled = response.Enabled
                    });
                }
                else
                {
                    return TypedResults.NotFound();
                }
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
            async (MockRouteDto route, MariaDbContext db, CancellationToken cancellationToken) =>
        {
            route.Method = (route.Method ?? string.Empty).ToUpper();

            if (HttpMethods.All(x => x != route.Method))
            {
                return Results.BadRequest($"{route.Method} is not a valid HTTP method");
            }

            var result = new MockRoute
            {
                RouteId = Guid.NewGuid().ToString(),
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

            route.RouteId = Guid.Parse(result.RouteId);

            return TypedResults.Created($"/prock/api/mock-routes/{result.RouteId}", route);
        });

        app.MapPut("/prock/api/mock-routes/{routeId}/disable-route",
            async Task<Results<Ok<MockRouteDto>, NotFound<Guid>>> (Guid routeId, MariaDbContext db, CancellationToken cancellationToken) =>
        {

            var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId.ToString());

            if (persistedRoute == null)
            {
                return TypedResults.NotFound(routeId);
            }

            persistedRoute.Enabled = false;

            await db.SaveChangesAsync(cancellationToken);

            app.Logger.LogInformation("Disabled route {Id}", persistedRoute.RouteId);

            var response = new MockRouteDto
            {
                RouteId = Guid.Parse(persistedRoute.RouteId),
                Method = persistedRoute.Method,
                Path = persistedRoute.Path,
                HttpStatusCode = persistedRoute.HttpStatusCode,
                Mock = persistedRoute.Mock != null ? JsonSerializer.Deserialize<dynamic>(persistedRoute.Mock) : null,
                Enabled = persistedRoute.Enabled
            };

            return TypedResults.Ok(response);
        });

        app.MapPut("/prock/api/mock-routes/{routeId}/enable-route",
            async Task<Results<Ok<MockRouteDto>, NotFound<Guid>>> (Guid routeId, MariaDbContext db, CancellationToken cancellationToken) =>
            {

                var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId.ToString());

                if (persistedRoute == null)
                {
                    return TypedResults.NotFound(routeId);
                }

                persistedRoute.Enabled = true;

                await db.SaveChangesAsync(cancellationToken);

                app.Logger.LogInformation("Disabled route {Id}", persistedRoute.RouteId);

                var response = new MockRouteDto
                {
                    RouteId = Guid.Parse(persistedRoute.RouteId),
                    Method = persistedRoute.Method,
                    Path = persistedRoute.Path,
                    HttpStatusCode = persistedRoute.HttpStatusCode,
                    Mock = JsonSerializer.Serialize(persistedRoute.Mock),
                    Enabled = persistedRoute.Enabled
                };

                return TypedResults.Ok(response);
            });

        app.MapPut("/prock/api/mock-routes", async Task<Results<Ok<MockRouteDto>, BadRequest<string>>> (MockRouteDto route, MariaDbContext db, CancellationToken cancellationToken) =>
        {

            if (HttpMethods.All(x => x != route.Method))
            {
                return TypedResults.BadRequest($"{route.Method} is not a valid HTTP method");
            }


            app.Logger.LogInformation("Updating {Path} ...", route.Path);
            var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == route.RouteId.ToString());
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

        app.MapDelete("/prock/api/mock-routes/{routeId}", async Task<Results<Ok, NotFound>> (Guid routeId, MariaDbContext db) =>
        {
            var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId.ToString());
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