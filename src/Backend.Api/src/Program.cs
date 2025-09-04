using System.Diagnostics;
using System.Net;
using Backend.Api.Endpoints;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetSection("Prock:MongoDbUri").Value ?? "mongodb://localhost:27017/";
var host = builder.Configuration.GetSection("Prock:Host").Value ?? "http://localhost";
var port = builder.Configuration.GetSection("Prock:Port").Value ?? "5001";
var dbName = builder.Configuration.GetSection("Prock:DbName").Value ?? "prock";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpForwarder();
builder.Services.AddSignalR();
builder.Services.AddCors();
var dbDataSource = new MongoClient(connectionString);
builder.Services.AddDbContext<ProckDbContext>(options => options.UseMongoDB(dbDataSource, dbName));
builder.Services.AddSingleton(new HttpMessageInvoker(new SocketsHttpHandler
{
    UseProxy = false,
    AllowAutoRedirect = false,
    AutomaticDecompression = DecompressionMethods.None,
    UseCookies = false,
    EnableMultipleHttp2Connections = true,
    ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
    ConnectTimeout = TimeSpan.FromSeconds(15),
}));
// Register services
builder.Services.AddScoped<IMockRouteService, MockRouteService>();
builder.Services.AddScoped<IProckConfigService, ProckConfigService>();
builder.Services.AddScoped<IOpenApiService, OpenApiService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opt => opt.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
{
    Title = "Prock API",
    Description = "Prock mocking proxy service with OpenAPI integration"
}));


var app = builder.Build();

app.UseOpenApi();
//app.UseSwagger();
//app.UseSwaggerUI();



app.UseCors(options =>
{
    options.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(origin => true);
});

app.UseRouting();

// Register SignalR hub
app.MapHub<Backend.Api.Endpoints.NotificationHub>("/prock/api/signalr");

// Register API endpoints
app.RegisterMockRouteEndpoints();
app.RegisterConfigurationEndpoints();
app.RegisterOpenApiDocumentEndpoints();
app.RegisterProxyEndpoints(); // Keep existing proxy functionality

// Health check endpoint for Docker
app.MapGet("/health", () => 
{
    return Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
}).WithTags("Health")
.WithName("HealthCheck")
.WithSummary("Health check endpoint for Docker and monitoring")
.Produces(200);

// Utility endpoints  
app.MapPost("/prock/api/restart", async Task<int> (CancellationToken cancellationToken) =>
{
    app.Logger.LogInformation("Restart called...");
    var currentProcess = Path.GetFullPath(Process.GetCurrentProcess().MainModule?.FileName ?? "Program.cs");
    app.Lifetime.StopApplication();
    Process.Start(currentProcess);
    return await Task.FromResult(0);
}).WithTags("Utilities");

app.MapPost("/prock/api/kill", (CancellationToken cancellationToken) => 
{
    app.Logger.LogInformation("Kill called...");
    app.Lifetime.StopApplication();
}).WithTags("Utilities");



app.Run($"{host}:{port}");
