using Yarp.ReverseProxy.Forwarder;

namespace DrunkenMaster.Net.Endpoints;

public static class MockEndpoints
{
    public static void RegisterMockEndpoints(this WebApplication app, HttpMessageInvoker httpClient)
    {
        
        app.Map("/{**catch-all}", async (HttpContext httpContext, IHttpForwarder forwarder) =>
        {
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