using System.Diagnostics;
using Core.Aggregates;
using Core.OpenTelemetry;
using Marten;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Repository;

public class MartenRepositoryWithTracingDecorator<T>: IMartenRepository<T>
    where T : class, IAggregate
{
    private readonly IMartenRepository<T> inner;
    private readonly IDocumentSession documentSession;
    private readonly IActivityScope activityScope;
    private readonly ILogger<MartenRepositoryWithTracingDecorator<T>> logger;

    public MartenRepositoryWithTracingDecorator(
        IMartenRepository<T> inner,
        IDocumentSession documentSession,
        IActivityScope activityScope,
        ILogger<MartenRepositoryWithTracingDecorator<T>> logger
    )
    {
        this.inner = inner;
        this.activityScope = activityScope;
        this.logger = logger;
        this.documentSession = documentSession;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    public Task<long> Add(T aggregate, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Add)}",
            (activity, ct) =>
            {
                PropagateTelemetry(activity);

                return inner.Add(aggregate, ct);
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            cancellationToken
        );

    public Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Update)}",
            (activity, ct) =>
            {
                PropagateTelemetry(activity);

                return inner.Update(aggregate, expectedVersion, ct);
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            token
        );

    public Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Delete)}",
            (activity, ct) =>
            {
                PropagateTelemetry(activity);

                return inner.Delete(aggregate, expectedVersion, ct);
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            token
        );

    private void PropagateTelemetry(Activity? activity)
    {
        var propagationContext = activity.Propagate(documentSession, InjectTelemetryIntoDocumentSession);

        if (!propagationContext.HasValue) return;

        documentSession.CorrelationId = propagationContext.Value.ActivityContext.TraceId.ToHexString();
        documentSession.CausationId = propagationContext.Value.ActivityContext.SpanId.ToHexString();
    }

    private void InjectTelemetryIntoDocumentSession(IDocumentSession session, string key, string value)
    {
        try
        {
            session.SetHeader(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to inject trace context");
        }
    }
}
