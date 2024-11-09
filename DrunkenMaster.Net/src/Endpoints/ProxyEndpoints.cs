using System.Diagnostics;
using System.Net;
using System.Text.Json;
using DrunkenMaster.Net.Data;
using DrunkenMaster.Net.Data.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Forwarder;

namespace DrunkenMaster.Net.Endpoints;

public static class ProxyEndpoints
{
    public static void RegisterProxyEndpoints(this WebApplication app)
    {

        app.Map("/{**catch-all}", async (HttpContext httpContext, IHttpForwarder forwarder, IHubContext<NotificationHub> hub, DrunkenMasterDbContext db, HttpMessageInvoker httpClient) =>
        {
            var requestPath = httpContext.Request.Path.Value;
            await hub.Clients.All.SendAsync("ProxyRequest", $"Request {requestPath}");
            var mock = await db.MockRoutes.SingleOrDefaultAsync(x => x.Path == requestPath);
            if (mock != null)
            {
                await hub.Clients.All.SendAsync("ProxyRequest", $"Mock: {mock.Path}");
                app.Logger.LogDebug("Mocking {Path}", mock.Path);
                var mockResponse = JsonSerializer.Deserialize<dynamic>(mock.Mock);
                return Results.Ok(mockResponse);
            }

            await hub.Clients.All.SendAsync("ProxyRequest", $"Proxy: {requestPath}");
            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
            var upstreamUrl = app.Configuration.GetSection("DrunkenMaster").GetSection("UpstreamUrl").Value ?? "https://example.com";

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