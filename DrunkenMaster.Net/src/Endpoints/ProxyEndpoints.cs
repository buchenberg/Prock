using System.Text.Json;
using DrunkenMaster.Net.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Forwarder;

namespace DrunkenMaster.Net.Endpoints;

public static class ProxyEndpoints
{
    public static void RegisterProxyEndpoints(this WebApplication app)
    {
        var upstreamUrl = app.Configuration.GetSection("DrunkenMaster").GetSection("UpstreamUrl").Value ?? "https://example.com";

        app.Map("/{**catch-all}", async (HttpContext httpContext, IHttpForwarder forwarder, IHubContext<NotificationHub> hub, DrunkenMasterDbContext db, HttpMessageInvoker httpClient) =>
        {
            var requestPath = httpContext.Request.Path.Value;
            var requestMethod = httpContext.Request.Method;
            await hub.Clients.All.SendAsync("ProxyRequest", $"Request {requestMethod} {requestPath}");

            var mock = await db.MockRoutes.SingleOrDefaultAsync(x => 
                x.Path.Equals(requestPath, StringComparison.CurrentCultureIgnoreCase)
                && x.Method.Equals(requestMethod, StringComparison.CurrentCultureIgnoreCase)
                && x.Enabled);

            if (mock != null)
            {       
                var mockResponse = JsonSerializer.Deserialize<dynamic>(mock.Mock);
                return Results.Ok(mockResponse);
            }

            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };

            var error = await forwarder.SendAsync(httpContext, upstreamUrl, httpClient, requestOptions, HttpTransformer.Default);
            if (error != ForwarderError.None)
            {
                var errorFeature = httpContext.GetForwarderErrorFeature();
                var exception = errorFeature?.Exception;
                app.Logger.LogError("Forwarding error: {@Exception}", exception);
            }

            return Task.CompletedTask;

        });

    }
}