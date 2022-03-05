using Core.Events;
using Core.Events.NoMediator;
using Core.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.ExternalProjections;

public class MartenExternalProjection<TEvent, TView>: INoMediatorEventHandler<EventEnvelope<TEvent>>
    where TView : IVersionedProjection
    where TEvent : notnull
{
    private readonly IDocumentSession session;
    private readonly Func<TEvent, Guid> getId;

    public MartenExternalProjection(
        IDocumentSession session,
        Func<TEvent, Guid> getId
    )
    {
        this.session = session;
        this.getId = getId;
    }

    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var entity = await session.LoadAsync<TView>(getId(eventEnvelope.Data), ct) ??
                     (TView)Activator.CreateInstance(typeof(TView), true)!;

        var eventLogPosition = eventEnvelope.Metadata.LogPosition;

        if (entity.LastProcessedPosition >= eventLogPosition)
            return;

        entity.When(eventEnvelope.Data);

        entity.LastProcessedPosition = eventLogPosition;

        session.Store(entity);

        await session.SaveChangesAsync(ct);
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
            .AddTransient<INoMediatorEventHandler<EventEnvelope<TEvent>>>(sp =>
            {
                var session = sp.GetRequiredService<IDocumentSession>();

                return new MartenExternalProjection<TEvent, TView>(session, getId);
            });

        return services;
    }
}
