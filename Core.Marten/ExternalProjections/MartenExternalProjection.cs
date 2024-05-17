using Core.Events;
using Core.Projections;
using Marten;
using Marten.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Core.Marten.ExternalProjections;

public class MartenExternalProjection<TEvent, TView>(
    IDocumentSession session,
    ILogger<MartenExternalProjection<TEvent, TView>> logger,
    Func<TEvent, Guid> getId
): IEventHandler<EventEnvelope<TEvent>>
    where TView : IVersionedProjection
    where TEvent : notnull
{
    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var (@event, eventMetadata) = eventEnvelope;

        var entity = await session.LoadAsync<TView>(getId(@event), ct).ConfigureAwait(false) ??
                     (TView)Activator.CreateInstance(typeof(TView), true)!;

        var eventLogPosition = eventMetadata.LogPosition;

        if (entity.LastProcessedPosition >= eventLogPosition)
            return;

        entity.Apply(@event);

        entity.LastProcessedPosition = eventLogPosition;

        session.Store(entity);

        try
        {
            await session.SaveChangesAsync(ct).ConfigureAwait(false);
        }
        catch (MartenException martenException)
            when (martenException.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            logger.LogWarning(martenException, "{ViewType} already exists. Ignoring", typeof(TView).Name);
        }
    }
}

public class MartenCheckpoint
{
    public string Id { get; set; } = default!;

    public ulong? Position { get; set; }

    public DateTime CheckpointedAt { get; set; } = default!;
}

public static class MartenExternalProjectionConfig
{
    public static IServiceCollection Project<TEvent, TView>(this IServiceCollection services,
        Func<TEvent, Guid> getId)
        where TView : class, IVersionedProjection
        where TEvent : notnull
    {
        services
            .AddTransient<IEventHandler<EventEnvelope<TEvent>>>(sp =>
            {
                var session = sp.GetRequiredService<IDocumentSession>();
                var logger = sp.GetRequiredService<ILogger<MartenExternalProjection<TEvent, TView>>>();

                return new MartenExternalProjection<TEvent, TView>(session, logger, getId);
            });

        return services;
    }
}
