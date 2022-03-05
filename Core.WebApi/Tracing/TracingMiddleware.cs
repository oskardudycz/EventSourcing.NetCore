using Core.Events;
using Core.Tracing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.WebApi.Tracing;

// Inspired by great Steve Gordon's work: https://github.com/stevejgordon/CorrelationId

public class TracingMiddleware
{
    private readonly RequestDelegate next;

    public TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
    }

    public async Task Invoke(
        HttpContext context,
        Func<IServiceProvider, TraceMetadata?, TracingScope> createTracingScope
    )
    {
        var traceMetadataFromHeaders = new TraceMetadata(
            context.Request.GetCorrelationId(),
            context.Request.GetCausationId()
        );
        using var tracingScope = createTracingScope(context.RequestServices, traceMetadataFromHeaders);

        var correlationId = tracingScope.CorrelationId;
        var causationId = tracingScope.CausationId;

        context.TraceIdentifier = correlationId.Value;

        // apply the correlation ID to the response header for client side tracking
        context.Response.OnStarting(() =>
        {
            context.Response.SetCorrelationId(correlationId);
            context.Response.SetCausationId(causationId);
            return Task.CompletedTask;
        });

        await next(context);
    }
}

public static class TracingMiddlewareConfig
{
    public static IServiceCollection AddCorrelationIdMiddleware(this IServiceCollection services) =>
        services.AddTracing();

    public static IApplicationBuilder UseCorrelationIdMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<TracingMiddleware>();
}
