using Core.Events;
using Core.Projections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace ECommerce.Core.Projections;

public class AddProjection<TView, TEvent, TDbContext>(
    TDbContext dbContext,
    Func<EventEnvelope<TEvent>, TView> create,
    ILogger<AddProjection<TView, TEvent, TDbContext>> logger
): IEventHandler<EventEnvelope<TEvent>>
    where TView : class
    where TDbContext : DbContext
    where TEvent : notnull
{
    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var view = create(eventEnvelope);

        try
        {
            await dbContext.AddAsync(view, ct);
            await dbContext.SaveChangesAsync(ct);
        }
        catch (Exception updateException)
            when (updateException.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            logger.LogWarning(updateException, "{ViewType} already exists. Ignoring", typeof(TView).Name);
        }
    }
}

public class UpdateProjection<TView, TEvent, TDbContext>(
    TDbContext dbContext,
    ILogger<AddProjection<TView, TEvent, TDbContext>> logger,
    Func<TEvent, object> getViewId,
    Action<EventEnvelope<TEvent>, TView> update
): IEventHandler<EventEnvelope<TEvent>>
    where TView : class
    where TDbContext : DbContext
    where TEvent : notnull
{
    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var viewId = getViewId(eventEnvelope.Data);
        var view = await dbContext.FindAsync<TView>([viewId], ct);

        switch (view)
        {
            case null:
                throw new InvalidOperationException($"{typeof(TView).Name} with id {viewId} wasn't found for event {typeof(TEvent).Name}");
            case ITrackLastProcessedPosition tracked when tracked.LastProcessedPosition <= eventEnvelope.Metadata.LogPosition:
                logger.LogWarning(
                    "{View} with id {ViewId} was already processed. LastProcessedPosition: {LastProcessedPosition}), event LogPosition: {LogPosition}",
                    typeof(TView).Name,
                    viewId,
                    tracked.LastProcessedPosition,
                    eventEnvelope.Metadata.LogPosition
                );
                return;
            case ITrackLastProcessedPosition tracked:
                tracked.LastProcessedPosition = eventEnvelope.Metadata.LogPosition;
                break;
        }

        update(eventEnvelope, view);

        await dbContext.SaveChangesAsync(ct);
    }
}
