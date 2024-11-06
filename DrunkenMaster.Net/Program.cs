using DrunkenMaster.Net.Data;
using DrunkenMaster.Net.Endpoints;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddHttpForwarder();

var app = builder.Build();

var connectionString = app.Configuration.GetSection("DrunkenMaster").GetSection("MongoDbUri").Value ?? "mongodb://localhost:27017/";
var client = new MongoClient(connectionString);
var db = DrunkenMasterDbContext.Create(client.GetDatabase("drunken-master"));

app.UseRouting();
app.RegisterProxyEndpoints();
app.RegisterMockEndpoints(db);
app.RegisterDrunkenMasterEndpoints(db);

var host = app.Configuration.GetSection("DrunkenMaster").GetSection("Host").Value ?? "http://localhost";
var port = app.Configuration.GetSection("DrunkenMaster").GetSection("Port").Value ?? "5000";

app.Run($"{host}:{port}");
