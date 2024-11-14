using System.Diagnostics;
using System.Net;
using backend.Data;
using backend.Endpoints;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Yarp.ReverseProxy.Forwarder;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetSection("DrunkenMaster:MongoDbUri").Value ?? "mongodb://localhost:27017/";
var host = builder.Configuration.GetSection("DrunkenMaster:Host").Value ?? "http://localhost";
var port = builder.Configuration.GetSection("DrunkenMaster:Port").Value ?? "5001";
var dbName = builder.Configuration.GetSection("DrunkenMaster:DbName").Value ?? "drunken-master";

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpForwarder();
builder.Services.AddSignalR();
builder.Services.AddCors();
var dbDataSource = new MongoClient(connectionString);
builder.Services.AddDbContext<DrunkenMasterDbContext>(options => options.UseMongoDB(dbDataSource, dbName));
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


var app = builder.Build();



app.UseCors(options =>
{
    options.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(origin => true);
});

app.UseRouting();
app.MapHub<NotificationHub>("/drunken-master/signalr");
app.RegisterDrunkenMasterEndpoints();
app.RegisterProxyEndpoints();


app.Run($"{host}:{port}");
