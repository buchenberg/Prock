using System.Diagnostics;
using System.Net;
using System.Text.Json;
using DrunkenMaster.Net;
using DrunkenMaster.Net.Data;
using DrunkenMaster.Net.Data.Dto;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Forwarder;
using MongoDB.Driver;

var cts = new CancellationTokenSource();

void OnShutdown()
{
    Console.WriteLine("OnShutdown");
}

void OnStopped()
{
    Console.WriteLine("OnStopped");
}


var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpForwarder();


var app = builder.Build();
app.Lifetime.ApplicationStopping.Register(OnShutdown);
app.Lifetime.ApplicationStopped.Register(OnStopped);


var connectionString = builder.Configuration.GetSection("DrunkenMaster").GetSection("MongoDbUri").Value ?? "mongodb://localhost:27017/";
var upstreamUrl = builder.Configuration.GetSection("DrunkenMaster").GetSection("UpstreamUrl").Value ?? "https://example.com";
var host = builder.Configuration.GetSection("DrunkenMaster").GetSection("Host").Value ?? "http://localhost";
var port = builder.Configuration.GetSection("DrunkenMaster").GetSection("Port").Value ?? "5000";


var client = new MongoClient(connectionString);
var db = DrunkenMasterDbContext.Create(client.GetDatabase("drunken-master"));


var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    EnableMultipleHttp2Connections = true,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
    ConnectTimeout = TimeSpan.FromSeconds(15),
});


var transformer = HttpTransformer.Default;
var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

app.UseRouting();

app.MapGet("/drunken-master/api/config", async (CancellationToken cancellationToken) =>
{
    return Results.Ok(new { connectionString, upstreamUrl, host, port });
});

app.Map("/{**catch-all}", async (HttpContext httpContext, IHttpForwarder forwarder) =>
{
    var error = await forwarder.SendAsync(httpContext, upstreamUrl,
        httpClient, requestOptions, transformer);
    // Check if the operation was successful
    if (error != ForwarderError.None)
    {
        var errorFeature = httpContext.GetForwarderErrorFeature();
        app.Logger.LogError("{Error}", errorFeature?.Exception);
    }
});


app.MapPost("/drunken-master/api/mock-routes", async (MockRouteDto route, CancellationToken cancellationToken) =>
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

    return Results.Created($"/drunken-master/api/mock-routes/{result.RouteId}", dto);
});

app.MapPut("/drunken-master/api/mock-routes", async (MockRouteDto route, CancellationToken cancellationToken) =>
{
    var methods = new[]
    {
        "Get", "Put", "Post", "Patch", "Delete"
    };
    if (methods.All(x => x != route.Method))
    {
        return Results.BadRequest($"{route.Method} is not a valid HTTP method");
    }
    

    app.Logger.LogInformation("Updating {Path} ...", route.Path);
    var persistedRoute = db.MockRoutes.SingleOrDefault(x => x.RouteId == route.RouteId);
    if (persistedRoute == null)
    {
        return Results.BadRequest($"Route {route.RouteId} not found");
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

    return Results.Ok(dto);
    
});

app.MapGet("/drunken-master/api/mock-routes", async (CancellationToken cancellationToken) =>
{
    var response = await db.MockRoutes.ToListAsync(cancellationToken);
    return response.Select(x => new MockRouteDto()
    {
        RouteId = x.RouteId,
        Method = x.Method,
        Path = x.Path,
        Mock = JsonSerializer.Deserialize<dynamic>(x.Mock)
    });
});

app.MapPost("/drunken-master/api/restart", async (CancellationToken cancellationToken) =>
{
    app.Logger.LogInformation("Restart called...");
    
    var currentProcess = Path.GetFullPath(Process.GetCurrentProcess().MainModule?.FileName ?? "Program.cs");
    app.Lifetime.StopApplication();
    Process.Start(currentProcess);
    return await Task.FromResult(0);
});

app.MapPost("/drunken-master/api/kill", async (CancellationToken cancellationToken) =>
{
    app.Lifetime.StopApplication();
});


app.MapGet("/drunken-master/api/mock-routes/{routeId}", (Guid routeId) =>
    {
        var response = db.MockRoutes.SingleOrDefault(x => x.RouteId == routeId);

        if (response != null)
        {
            return Results.Ok(new MockRouteDto()
            {
                RouteId = response.RouteId,
                Method = response.Method,
                Path = response.Path,
                Mock = JsonSerializer.Deserialize<dynamic>(response.Mock)
            });
        }

        return Results.NotFound();
    }

);



foreach (var route in db.MockRoutes)
{
    app.Logger.LogInformation("Configuring {Path} ...", route.Path);
    var mock = JsonSerializer.Deserialize<dynamic>(route.Mock);
    app.MapMethods(route.Path, new[] { route.Method.ToUpper() },
        () => mock);

}


app.Run($"{host}:{port}");
