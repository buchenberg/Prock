using System.Diagnostics;
using System.Net;
using backend.Data;
using backend.Endpoints;
using backend.Repositories;
using backend.Services;
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IOpenApiRepository, OpenApiRepository>();
builder.Services.AddScoped<IOpenApiService, OpenApiService>();
builder.Services.AddScoped<IMockRouteRepository, MockRouteRepository>();
builder.Services.AddScoped<IProckConfigRepository, ProckConfigRepository>();
builder.Services.AddSwaggerGen(opt => opt.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
{
    Title = "Prock API",
    Description = "Description of the service"
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
app.MapHub<NotificationHub>("/prock/api/signalr");
app.RegisterProckEndpoints();
app.RegisterProxyEndpoints();
app.RegisterConfigEndpoints();
app.RegisterOpenApiEndpoints();



app.Run($"{host}:{port}");
