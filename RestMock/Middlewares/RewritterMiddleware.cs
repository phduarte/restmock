using Microsoft.AspNetCore.Http.Extensions;
using RestMock.Domain;
using System.Text;

namespace RestMock.Middlewares;

public class RewritterMiddleware
{
    private static readonly string[] ProtectedPrefixes = new[]
    {
        "/client",
        "/mocks",
        "/swagger",
        "/_blazor",
        "/_framework",
        "/_content",
        "/css",
        "/js",
        "/favicon",
    };

    private readonly RequestDelegate _next;

    public RewritterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (ProtectedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var url = context.Request.GetDisplayUrl();

        if (EndpointCollection.Find(context.Request.Method, url) is EndpointModel endpoint)
        {
            await Task.Delay(endpoint.ProcessingTime);
            context.Response.StatusCode = endpoint.StatusCode;
            context.Response.ContentType = endpoint.ContentType;

            if (endpoint.ResponseBody != null)
            {
                string? requestBody = null;
                if (context.Request.ContentLength > 0 || context.Request.Headers.ContainsKey("Transfer-Encoding"))
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                var responseBody = ResponseTemplateProcessor.Process(endpoint.ResponseBody.ToString()!, requestBody);
                await context.Response.WriteAsync(responseBody);
            }
        }
        else
        {
            await _next(context);
        }
    }
}
