using Microsoft.AspNetCore.Http.Extensions;
using RestMock.Domain;

namespace RestMock.Middlewares;

public class RewritterMiddleware
{
    private readonly RequestDelegate _next;

    public RewritterMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var url = context.Request.GetDisplayUrl();

        if (EndpointCollection.Find(context.Request.Method, url) is EndpointModel endpoint)
        {
            await Task.Delay(endpoint.ProcessingTime);
            context.Response.StatusCode = endpoint.StatusCode;
            context.Response.ContentType = endpoint.ContentType;

            if (endpoint.ResponseBody != null)
            {
                await context.Response.WriteAsync(endpoint.ResponseBody.ToString());
            }
        }
        else
        {
            await _next(context);
        }
    }
}
