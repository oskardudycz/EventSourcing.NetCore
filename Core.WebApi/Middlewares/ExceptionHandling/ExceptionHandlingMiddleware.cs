using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.WebApi.Middlewares.ExceptionHandling;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate next;

    private readonly ILogger logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILoggerFactory loggerFactory
    )
    {
        this.next = next;
        logger = loggerFactory.CreateLogger<ExceptionHandlingMiddleware>();
    }

    public async Task Invoke(HttpContext context /* other scoped dependencies */)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, exception.Message);
        Console.WriteLine(exception.Message);

        var codeInfo = ExceptionToHttpStatusMapper.Map(exception);

        var result = JsonConvert.SerializeObject(new HttpExceptionWrapper((int)codeInfo.Code, codeInfo.Message));
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)codeInfo.Code;
        return context.Response.WriteAsync(result);
    }
}

public class HttpExceptionWrapper
{
    public int StatusCode { get; }

    public string Error { get; }

    public HttpExceptionWrapper(int statusCode, string error)
    {
        StatusCode = statusCode;
        Error = error;
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(
        this IApplicationBuilder app,
        Func<Exception, HttpStatusCode>? customMap = null
    )
    {
        ExceptionToHttpStatusMapper.CustomMap = customMap;
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}