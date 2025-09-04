using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace backend.Tests.TestBase;

/// <summary>
/// Helper for creating mock HttpContext objects for testing
/// </summary>
public static class MockHttpContext
{
    /// <summary>
    /// Creates a mock HttpContext with the specified request details
    /// </summary>
    public static HttpContext Create(
        string method = "GET",
        string path = "/test",
        IDictionary<string, StringValues>? headers = null,
        IDictionary<string, StringValues>? queryString = null)
    {
        var context = new DefaultHttpContext();
        
        context.Request.Method = method;
        context.Request.Path = path;
        
        if (headers != null)
        {
            foreach (var header in headers)
            {
                context.Request.Headers[header.Key] = header.Value;
            }
        }
        
        if (queryString != null)
        {
            var query = new QueryCollection((Dictionary<string, StringValues>)queryString);
            context.Request.Query = query;
        }
        
        return context;
    }

    /// <summary>
    /// Creates a mock HttpContext with JSON body content
    /// </summary>
    public static HttpContext CreateWithJsonBody(object body, string method = "POST", string path = "/test")
    {
        var context = Create(method, path);
        var json = JsonSerializer.Serialize(body);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        
        context.Request.Body = new MemoryStream(bytes);
        context.Request.ContentType = "application/json";
        context.Request.ContentLength = bytes.Length;
        
        return context;
    }
}
