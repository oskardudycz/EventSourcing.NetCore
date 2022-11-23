using Core.Aggregates;
using Core.OpenTelemetry;
using Core.Tracing;
using Marten;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Core.Marten.Repository;

public interface IMartenRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task<long> Add(T aggregate, TraceMetadata? eventMetadata = null, CancellationToken cancellationToken = default);

    Task<long> Update(T aggregate, long? expectedVersion = null, TraceMetadata? traceMetadata = null,
        CancellationToken cancellationToken = default);

    Task<long> Delete(T aggregate, long? expectedVersion = null, TraceMetadata? eventMetadata = null,
        CancellationToken cancellationToken = default);
}

public class MartenRepository<T>: IMartenRepository<T> where T : class, IAggregate
{
    private readonly IDocumentSession documentSession;
    private readonly IActivityScope activityScope;
    private readonly ILogger<MartenRepository<T>> logger;
    private readonly TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

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

    public Task<long> Add(T aggregate, TraceMetadata? traceMetadata = null,
        CancellationToken cancellationToken = default) =>
        activityScope.Run($"{typeof(MartenRepository<T>).Name}/{nameof(Add)}",
            async (activity, ct) =>
            {
                //     documentSession.CorrelationId = activity?.TraceId.ToHexString();
                //     documentSession.CausationId = activity?.HasRemoteParent == true ? activity.ParentId : null;
                documentSession.CorrelationId = traceMetadata?.CorrelationId?.Value;
                documentSession.CausationId = traceMetadata?.CausationId?.Value;

                if (activity?.Context != null)
                {
                    propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), documentSession,
                        InjectTraceContextIntoBasicProperties);
                }

                var events = aggregate.DequeueUncommittedEvents();

                documentSession.Events.StartStream<Aggregate>(
                    aggregate.Id,
                    events
                );

                await documentSession.SaveChangesAsync(ct);

                return (long)events.Length;
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            cancellationToken
        );

    public Task<long> Update(T aggregate, long? expectedVersion = null, TraceMetadata? traceMetadata = null,
        CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Add)}",
            async (activity, ct) =>
            {
                documentSession.CorrelationId = traceMetadata?.CorrelationId?.Value;
                documentSession.CausationId = traceMetadata?.CausationId?.Value;

                if (activity?.Context != null)
                {
                    propagator.Inject(new PropagationContext(activity.Context, Baggage.Current), documentSession,
                        InjectTraceContextIntoBasicProperties);
                }

                var events = aggregate.DequeueUncommittedEvents();

                var nextVersion = (expectedVersion ?? aggregate.Version) + events.Length;

                documentSession.Events.Append(
                    aggregate.Id,
                    nextVersion,
                    events
                );

                await documentSession.SaveChangesAsync(ct);

                return nextVersion;
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            cancellationToken
        );

    public Task<long> Delete(T aggregate, long? expectedVersion = null, TraceMetadata? traceMetadata = null,
        CancellationToken cancellationToken = default) =>
        Update(aggregate, expectedVersion, traceMetadata, cancellationToken);

    private void InjectTraceContextIntoBasicProperties(IDocumentSession session, string key, string value)
    {
        try
        {
            session.SetHeader(key, value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to inject trace context.");
        }
    }
}
