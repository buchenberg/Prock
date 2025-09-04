using System.Diagnostics;
using System.Net;
using Backend.Api.Endpoints;
using Backend.Core.Services.Interfaces;
using Backend.Infrastructure.Data.Context;
using Backend.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);
var host = builder.Configuration.GetSection("Prock:Host").Value ?? "http://localhost";
var port = builder.Configuration.GetSection("Prock:Port").Value ?? "5001";

// MariaDB configuration
var mariaConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
    builder.Configuration.GetConnectionString("MariaDbConnectionString") ?? 
    "server=localhost;user id=root;password=mockula;database=mockula";
var serverVersion = new MySqlServerVersion(new Version(10, 5, 4));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpForwarder();
builder.Services.AddSignalR();
builder.Services.AddCors();

// Configure MariaDB (only database now)
builder.Services.AddDbContext<MariaDbContext>(options => 
    options.UseMySql(mariaConnectionString, serverVersion, options => 
        {
            options.EnableStringComparisonTranslations();
        })
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors());
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
// Register MariaDB services (all data now stored in MariaDB)
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

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<MariaDbContext>();
    try
    {
        // Create database if it doesn't exist and apply migrations
        dbContext.Database.Migrate();
        app.Logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while applying database migrations.");
        // Don't fail startup for database connection issues in some environments
    }
}

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
