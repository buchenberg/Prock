using backend.Repositories;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.SignalR;
using Yarp.ReverseProxy.Forwarder;

namespace backend.Endpoints;

public static class ProxyEndpoints
{
    public static void RegisterProxyEndpoints(this WebApplication app)
    {
        var defaultUpstreamUrl = app.Configuration.GetSection("Prock").GetSection("UpstreamUrl").Value ?? "https://example.com";

        app.Map("/{**catch-all}", async Task<Results<ContentHttpResult, ProblemHttpResult, EmptyHttpResult>> (HttpContext httpContext, IHttpForwarder forwarder, IHubContext<NotificationHub> hub, IMockRouteRepository routeRepo, IProckConfigRepository configRepo, HttpMessageInvoker httpClient) =>
        {
            var requestPath = httpContext.Request.Path.Value;
            var requestMethod = httpContext.Request.Method;
            await hub.Clients.All.SendAsync("ProxyRequest", $"Request {requestMethod} {requestPath}");

            var mock = await routeRepo.FindMatchingRouteAsync(requestPath, requestMethod);

            if (mock != null)
            {

                return TypedResults.Content(content: System.Text.Json.JsonSerializer.Serialize(mock.Mock), contentType: "application/json", statusCode: mock.HttpStatusCode);
            }

            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
            var config = await configRepo.GetConfigAsync();
            var upstreamUrl = config?.UpstreamUrl ?? defaultUpstreamUrl;

            var error = await forwarder.SendAsync(httpContext, upstreamUrl, httpClient, requestOptions, HttpTransformer.Default);
            if (error != ForwarderError.None)
            {
                var errorFeature = httpContext.GetForwarderErrorFeature();
                var exception = errorFeature?.Exception;
                app.Logger.LogError("Forwarding error: {@Exception}", exception);
                return TypedResults.Problem("Forwarding error");
            }

            return TypedResults.Empty;

        });

    }
}