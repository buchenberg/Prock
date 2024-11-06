using System.Diagnostics;
using System.Net;
using Yarp.ReverseProxy.Forwarder;

namespace DrunkenMaster.Net.Endpoints;

public static class ProxyEndpoints
{
    public static void RegisterProxyEndpoints(this WebApplication app)
    {
        app.Map("/{**catch-all}", async (HttpContext httpContext, IHttpForwarder forwarder) =>
        {
            var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
            {
                UseProxy = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None,
                UseCookies = false,
                EnableMultipleHttp2Connections = true,
                ActivityHeadersPropagator = new ReverseProxyPropagator(DistributedContextPropagator.Current),
                ConnectTimeout = TimeSpan.FromSeconds(15),
            });
            var transformer = HttpTransformer.Default;
            var requestOptions = new ForwarderRequestConfig { ActivityTimeout = TimeSpan.FromSeconds(100) };
            var upstreamUrl = app.Configuration.GetSection("DrunkenMaster").GetSection("UpstreamUrl").Value ?? "https://example.com";
            var error = await forwarder.SendAsync(httpContext, upstreamUrl,
                httpClient, requestOptions, transformer);
            // Check if the operation was successful
            if (error != ForwarderError.None)
            {
                var errorFeature = httpContext.GetForwarderErrorFeature();
                app.Logger.LogError("{Error}", errorFeature?.Exception);
            }
        });
        
    }
}