using Core.Tracing;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Core.WebApi.Tracing;

// Inspired by great Steve Gordon's work: https://github.com/stevejgordon/CorrelationId

public class TracingMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";
    private const string CausationIdHeaderName = "X-Causation-ID";

    private readonly RequestDelegate next;
    private readonly ILogger<TracingMiddleware> logger;

    public TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
        this.logger = logger;
    }

    public async Task Invoke(
        HttpContext context,
        Func<IServiceProvider, TracingScope> createTracingScope
    )
    {
        using var tracingScope = createTracingScope(context.RequestServices);
        var correlationId = tracingScope.CorrelationId;
        var causationId = tracingScope.CausationId;

        // apply the correlation ID to the response header for client side tracking
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(CorrelationIdHeaderName, new[] { correlationId.Value });
            context.Response.Headers.Add(CausationIdHeaderName, new[] { causationId.Value });
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
