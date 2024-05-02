using Core.Aggregates;
using Core.Marten.OpenTelemetry;
using Core.OpenTelemetry;
using Marten;
using Microsoft.Extensions.Logging;

namespace Core.Marten.Repository;

public class MartenRepositoryWithTracingDecorator<T>(
    IMartenRepository<T> inner,
    IDocumentSession documentSession,
    IActivityScope activityScope,
    ILogger<MartenRepositoryWithTracingDecorator<T>> logger)
    : IMartenRepository<T>
    where T : class, IAggregate
{
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    public Task<long> Add(T aggregate, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Add)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);

                return inner.Add(aggregate, ct);
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            cancellationToken
        );

    public Task<long> Update(T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Update)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);

                return inner.Update(aggregate, expectedVersion, ct);
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            token
        );

    public Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Delete)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);

                return inner.Delete(aggregate, expectedVersion, ct);
            },
            new StartActivityOptions { Tags = { { TelemetryTags.Logic.Entity, typeof(T).Name } } },
            token
        );
}
