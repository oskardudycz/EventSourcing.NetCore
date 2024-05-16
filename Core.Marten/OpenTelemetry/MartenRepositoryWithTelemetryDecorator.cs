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

    public Task<long> Add(Guid id, T aggregate, CancellationToken cancellationToken = default) =>
        activityScope.Run($"MartenRepository/{nameof(Add)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);

                return inner.Add(id, aggregate, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version }
                }
            },
            cancellationToken
        );

    public Task<long> Update(Guid id, T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Update)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);

                return inner.Update(id, aggregate, expectedVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version }
                }
            },
            token
        );

    public Task<long> Delete(Guid id, T aggregate, long? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"MartenRepository/{nameof(Delete)}",
            (activity, ct) =>
            {
                documentSession.PropagateTelemetry(activity, logger);

                return inner.Delete(id, aggregate, expectedVersion, ct);
            },
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version }
                }
            },
            token
        );
}
