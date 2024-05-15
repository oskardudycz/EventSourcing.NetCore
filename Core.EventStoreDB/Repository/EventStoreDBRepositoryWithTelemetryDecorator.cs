using Core.Aggregates;
using Core.OpenTelemetry;
using Microsoft.Extensions.Logging;

namespace Core.EventStoreDB.Repository;

public class EventStoreDBRepositoryWithTelemetryDecorator<T>(
    IEventStoreDBRepository<T> inner,
    IActivityScope activityScope)
    : IEventStoreDBRepository<T>
    where T : class, IAggregate
{
    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    public Task<ulong> Add(T aggregate, CancellationToken cancellationToken = default) =>
        activityScope.Run($"EventStoreDBRepository/{nameof(Add)}",
            (_, ct) => inner.Add(aggregate, ct),
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, aggregate.Id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version }
                }
            },
            cancellationToken
        );

    public Task<ulong> Update(T aggregate, ulong? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"EventStoreDBRepository/{nameof(Update)}",
            (_, ct) => inner.Update(aggregate, expectedVersion, ct),
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, aggregate.Id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version }
                }
            },
            token
        );

    public Task<ulong> Delete(T aggregate, ulong? expectedVersion = null, CancellationToken token = default) =>
        activityScope.Run($"EventStoreDBRepository/{nameof(Delete)}",
            (_, ct) => inner.Delete(aggregate, expectedVersion, ct),
            new StartActivityOptions
            {
                Tags =
                {
                    { TelemetryTags.Logic.Entities.EntityType, typeof(T).Name },
                    { TelemetryTags.Logic.Entities.EntityId, aggregate.Id },
                    { TelemetryTags.Logic.Entities.EntityVersion, aggregate.Version }
                }
            },
            token
        );
}
