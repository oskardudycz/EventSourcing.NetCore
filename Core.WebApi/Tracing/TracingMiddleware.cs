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

    private const string CorrelationIdLoggerScopeKey = "Correlation-ID";
    private const string CausationIdLoggerScopeKey = "Causation-ID";

    private readonly RequestDelegate next;
    private readonly ILogger<TracingMiddleware> logger;

    public TracingMiddleware(RequestDelegate next, ILogger<TracingMiddleware> logger)
    {
        this.next = next ?? throw new ArgumentNullException(nameof(next));
        this.logger = logger;
    }

    public async Task Invoke(
        HttpContext context,
        ICorrelationIdFactory correlationIdFactory,
        ICorrelationIdProvider correlationIdProvider,
        ICausationIdProvider causationIdProvider
    )
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationIdHeader)
            ? new CorrelationId(correlationIdHeader)
            : correlationIdFactory.New();

        context.TraceIdentifier = correlationId.Value;
        correlationIdProvider.Set(correlationId);

        var causationId = context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var causationIdHeader)
            ? new CausationId(causationIdHeader)
            : new CausationId(correlationId.Value);

        causationIdProvider.Set(causationId);

        // apply the correlation ID to the response header for client side tracking
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(CorrelationIdHeaderName, new[] { correlationId.Value });
            context.Response.Headers.Add(CausationIdHeaderName, new[] { causationId.Value });
            return Task.CompletedTask;
        });

        // add CorrelationId explicitly to the logger scope
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationIdLoggerScopeKey] = correlationId, [CausationIdLoggerScopeKey] = causationId
        });

        await next(context);
    }
}

public static class TracingMiddlewareConfig
{
    public static IServiceCollection AddCorrelationIdMiddleware(this IServiceCollection services)
    {
        services.TryAddScoped<ICorrelationIdFactory, GuidCorrelationIdFactory>();
        services.TryAddScoped<ICausationIdFactory, GuidCausationIdFactory>();
        services.TryAddScoped<ICorrelationIdProvider, CorrelationIdProvider>();
        services.TryAddScoped<ICausationIdProvider, CausationIdProvider>();

        return services;
    }

    public static IApplicationBuilder UseCorrelationIdMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<TracingMiddleware>();
}
