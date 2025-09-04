using System.Text.Json;
using Backend.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Yarp.ReverseProxy.Forwarder;

namespace Backend.Api.Endpoints;

public static class ProxyEndpoints
{
    public static void RegisterProxyEndpoints(this WebApplication app)
    {
        var defaultUpstreamUrl = app.Configuration.GetSection("Prock").GetSection("UpstreamUrl").Value ?? "https://example.com";

        app.MapFallback(async Task<Results<ContentHttpResult, ProblemHttpResult>> (HttpContext httpContext, IHttpForwarder forwarder, IHubContext<NotificationHub> hub, MariaDbContext db, HttpMessageInvoker httpClient) =>
        {
            var requestPath = httpContext.Request.Path.Value;
            var requestMethod = httpContext.Request.Method;
            await hub.Clients.All.SendAsync("ProxyRequest", $"Request {requestMethod} {requestPath}");

            var mock = await db.MockRoutes.SingleOrDefaultAsync(x =>
                string.Equals(x.Path, requestPath, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(x.Method, requestMethod, StringComparison.CurrentCultureIgnoreCase)
                && x.Enabled);

            if (mock != null)
            {

                return TypedResults.Content(content: mock.Mock, contentType: "application/json", statusCode: mock.HttpStatusCode);
            }

            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
            var config = await db.ProckConfigs.SingleOrDefaultAsync();
            var upstreamUrl = config?.UpstreamUrl ?? defaultUpstreamUrl;

            var error = await forwarder.SendAsync(httpContext, upstreamUrl, httpClient, requestOptions, HttpTransformer.Default);
            if (error != ForwarderError.None)
            {
                var errorFeature = httpContext.GetForwarderErrorFeature();
                var exception = errorFeature?.Exception;
                app.Logger.LogError("Forwarding error: {@Exception}", exception);
                return TypedResults.Problem("Forwarding error");
            }

            return TypedResults.Problem("Forwarder error occurred", statusCode: 500);

        });

    }
}