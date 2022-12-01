using System.Diagnostics;
using Core.Aggregates;
using Core.OpenTelemetry;
using Marten;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Repository;

public interface IMartenRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<long> Add(T aggregate, CancellationToken cancellationToken = default);
    Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
    Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default);
}

public class MartenRepository<T>: IMartenRepository<T> where T : class, IAggregate
{
    private readonly IDocumentSession documentSession;
    private readonly IActivityScope activityScope;
    private readonly ILogger<MartenRepository<T>> logger;

    public MartenRepository(
        IDocumentSession documentSession,
        IActivityScope activityScope,
        ILogger<MartenRepository<T>> logger
    )
    {
        this.documentSession = documentSession;
        this.activityScope = activityScope;
        this.logger = logger;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        documentSession.Events.AggregateStreamAsync<T>(id, token: cancellationToken);

    public Task<long> Add(T aggregate, CancellationToken cancellationToken = default) =>
        activityScope.Run($"{typeof(MartenRepository<T>).Name}/{nameof(Add)}",
            async (activity, ct) =>
            {
                PropagateTelemetry(activity);

                var events = aggregate.DequeueUncommittedEvents();

                documentSession.Events.StartStream<Aggregate>(
                    aggregate.Id,
                    events
                );

                await documentSession.SaveChangesAsync(ct).ConfigureAwait(false);

                return (long)events.Length;
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            cancellationToken
        );

    public Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Update)}",
            async (activity, ct) =>
            {
                PropagateTelemetry(activity);

                var events = aggregate.DequeueUncommittedEvents();

                var nextVersion = (expectedVersion ?? aggregate.Version) + events.Length;

                documentSession.Events.Append(
                    aggregate.Id,
                    nextVersion,
                    events
                );

                await documentSession.SaveChangesAsync(ct).ConfigureAwait(false);

                return nextVersion;
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            token
        );

    public Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Delete)}",
            async (activity, ct) =>
            {
                PropagateTelemetry(activity);

                var events = aggregate.DequeueUncommittedEvents();

                var nextVersion = (expectedVersion ?? aggregate.Version) + events.Length;

                documentSession.Events.Append(
                    aggregate.Id,
                    nextVersion,
                    events
                );

                await documentSession.SaveChangesAsync(ct).ConfigureAwait(false);

                return nextVersion;
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
