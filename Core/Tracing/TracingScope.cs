using Core.Events;
using Core.Tracing.Causation;
using Core.Tracing.Correlation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Tracing;

public class TracingScope: IDisposable
{
    public CorrelationId CorrelationId { get; }
    public CausationId CausationId { get; }
    private readonly IDisposable? loggerScope;

    public TracingScope(IDisposable? loggerScope, CorrelationId correlationId, CausationId causationId)
    {
        this.loggerScope = loggerScope;
        CorrelationId = correlationId;
        CausationId = causationId;
    }

    public void Dispose()
    {
        loggerScope?.Dispose();
    }
}

public interface ITracingScopeFactory
{
    TracingScope CreateTraceScope(IServiceProvider serviceProvider, TraceMetadata? traceMetadata = null);
}

public class TracingScopeFactory: ITracingScopeFactory
{
    private readonly ILogger<TracingScopeFactory> logger;
    private readonly ICorrelationIdFactory correlationIdFactory;

    public TracingScopeFactory(
        ILogger<TracingScopeFactory> logger,
        ICorrelationIdFactory correlationIdFactory
    )
    {
        this.logger = logger;
        this.correlationIdFactory = correlationIdFactory;
    }

    public TracingScope CreateTraceScope(IServiceProvider serviceProvider, TraceMetadata? traceMetadata = null)
    {
        var correlationId = traceMetadata?.CorrelationId ?? correlationIdFactory.New();

        var correlationIdProvider = serviceProvider.GetRequiredService<ICorrelationIdProvider>();
        correlationIdProvider.Set(correlationId);

        // if Causation Id was not provided, use Correlation Id
        var causationId = traceMetadata?.CausationId ?? new CausationId(correlationId.Value);

        var causationIdProvider = serviceProvider.GetRequiredService<ICausationIdProvider>();
        causationIdProvider.Set(causationId);

        // TODO: Add logger
        var loggerScope = logger.BeginScope(new Dictionary<string, object>
        {
            [CorrelationId.LoggerScopeKey] = correlationId.Value, [CausationId.LoggerScopeKey] = causationId.Value
        });

        return new TracingScope(loggerScope, correlationId, causationId);
    }
}

public static class TraceScopeFactoryExtensions
{
    public static TracingScope CreateTraceScope(
        this ITracingScopeFactory tracingScopeFactory,
        IServiceProvider serviceProvider, EventEnvelope? eventEnvelope)
    {
        if (eventEnvelope == null)
            return tracingScopeFactory.CreateTraceScope(serviceProvider);

        var (_, eventMetadata) = eventEnvelope;

        var newCausationId = new CausationId(eventMetadata.EventId);

        return tracingScopeFactory.CreateTraceScope(
            serviceProvider,
            new TraceMetadata(eventMetadata.Trace?.CorrelationId, newCausationId)
        );
    }
}
