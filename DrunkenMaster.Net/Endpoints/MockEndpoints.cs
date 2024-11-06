using System.Text.Json;
using DrunkenMaster.Net.Data;

namespace DrunkenMaster.Net.Endpoints;

public static class ProxyEndpoints
{

    public static void RegisterProxyEndpoints(this WebApplication app, DrunkenMasterDbContext db)
    {
        
        foreach (var route in db.MockRoutes)
        {
            app.Logger.LogInformation("Configuring {Path} ...", route.Path);
            var mock = JsonSerializer.Deserialize<dynamic>(route.Mock);
            app.MapMethods(route.Path, new[] { route.Method.ToUpper() },
                () => mock);
        }
        
    }
    
}