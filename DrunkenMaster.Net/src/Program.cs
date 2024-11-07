using DrunkenMaster.Net.Data;
using DrunkenMaster.Net.Endpoints;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpForwarder();

var app = builder.Build();

var connectionString = "mongodb://localhost:27017/";
//"https://gbuchenberger.na1.sa.allcovered.com";// Environment.GetEnvironmentVariable("CONN_STR") ?? "mongodb://user:pass@mongodb";//app.Configuration.GetSection("DrunkenMaster").GetSection("MongoDbUri").Value ?? "mongodb:mongodb";
var client = new MongoClient(connectionString);
var db = DrunkenMasterDbContext.Create(client.GetDatabase("drunken-master"));

app.UseRouting();
app.RegisterProxyEndpoints();
app.RegisterMockEndpoints(db);
app.RegisterDrunkenMasterEndpoints(db);

var host = app.Configuration.GetSection("DrunkenMaster").GetSection("Host").Value ?? "http://localhost";
var port = app.Configuration.GetSection("DrunkenMaster").GetSection("Port").Value ?? "5001";

app.Run("http://localhost:5001");
