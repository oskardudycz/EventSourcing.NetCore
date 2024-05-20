using System.Linq.Expressions;
using Core.EntityFramework.Extensions;
using Core.Events;
using Core.Reflection;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.EntityFramework.Projections;

public class EntityFrameworkProjection<TDbContext>: IEventBatchHandler
    where TDbContext : DbContext
{
    public TDbContext DbContext { protected get; set; } = default!;
    public IAsyncPolicy RetryPolicy { protected get; set; } = Policy.NoOpAsync();

    private readonly HashSet<Type> handledEventTypes = [];

    protected void Projects<TEvent>() =>
        handledEventTypes.Add(typeof(TEvent));

    public async Task Handle(IEventEnvelope[] eventInEnvelopes, CancellationToken ct)
    {
        var events = eventInEnvelopes
            .Where(@event => handledEventTypes.Contains(@event.Data.GetType()))
            .ToArray();

        await ApplyAsync(events, ct);
    }

    protected virtual Task ApplyAsync(IEventEnvelope[] events, CancellationToken ct) =>
        ApplyAsync(events.Select(@event => @event.Data).ToArray(), ct);

    protected virtual Task ApplyAsync(object[] events, CancellationToken ct) =>
        Task.CompletedTask;
}

public class EntityFrameworkProjection<TView, TId, TDbContext>: EntityFrameworkProjection<TDbContext>
    where TView : class
    where TId : struct
    where TDbContext : DbContext
{
    public Expression<Func<TView, object>>? Include { protected get; set; }

    private record ProjectEvent(
        Func<IEventEnvelope, TId?> GetId,
        Func<TView, IEventEnvelope, TView?> Apply
    );

    private readonly Dictionary<Type, ProjectEvent> projectors = new();
    private Expression<Func<TView, TId>> viewIdExpression = default!;
    private Func<TView, TId> viewId = default!;

    public void ViewId(Expression<Func<TView, TId>> id)
    {
        viewIdExpression = id;
        viewId = id.Compile();
    }

    public void Creates<TEvent>(
        Func<EventEnvelope<TEvent>, TView> apply
    ) where TEvent : notnull =>
        Projects<TEvent>(
            new ProjectEvent(
                _ => null,
                (_, envelope) => apply((EventEnvelope<TEvent>)envelope)
            )
        );

    public void Creates<TEvent>(
        Func<TEvent, TView> apply
    ) where TEvent : notnull =>
        Creates<TEvent>(envelope => apply(envelope.Data));

    public void Deletes<TEvent>(
        Func<TEvent, TId> getId
    ) =>
        Projects<TEvent>(
            new ProjectEvent(
                envelope => getId((TEvent)envelope.Data),
                (_, _) => null
            )
        );

    public void Projects<TEvent>(
        Func<TEvent, TId> getId,
        Func<TView, EventEnvelope<TEvent>, TView> apply
    ) where TEvent : notnull =>
        Projects<TEvent>(
            new ProjectEvent(
                envelope => getId((TEvent)envelope.Data),
                (document, envelope) => apply(document, (EventEnvelope<TEvent>)envelope)
            )
        );

    public void Projects<TEvent>(
        Func<TEvent, TId> getId,
        Action<TView, EventEnvelope<TEvent>> apply
    ) where TEvent : notnull =>
        Projects(getId, (view, envelope) =>
        {
            apply(view, envelope);
            return view;
        });

    public void Projects<TEvent>(
        Func<TEvent, TId> getId,
        Func<TView, TEvent, TView> apply
    ) where TEvent : notnull =>
        Projects(getId, (view, envelope) => apply(view, envelope.Data));

    public void Projects<TEvent>(
        Func<TEvent, TId> getId,
        Action<TView, TEvent> apply
    ) where TEvent : notnull =>
        Projects(getId, (view, envelope) => apply(view, envelope.Data));


    private void Projects<TEvent>(ProjectEvent projectEvent)
    {
        projectors.Add(typeof(TEvent), projectEvent);
        Projects<TEvent>();
    }

    private TView? Apply(TView document, IEventEnvelope @event) =>
        projectors[@event.Data.GetType()].Apply(document, @event);

    private TId? GetViewId(IEventEnvelope @event) =>
        projectors[@event.Data.GetType()].GetId(@event);

    protected override Task ApplyAsync(IEventEnvelope[] events, CancellationToken token) =>
        RetryPolicy.ExecuteAsync(async ct =>
        {
            var dbSet = DbContext.Set<TView>();

            var eventWithViewIds = events.Select(e => (Event: e, ViewId: GetViewId(e))).ToList();
            var ids = eventWithViewIds.Where(e => e.ViewId.HasValue).Select(e => e.ViewId!.Value).ToList();

            var existingViews = await GetExistingViews(dbSet, ids, ct);

            foreach (var (eventEnvelope, id) in eventWithViewIds)
            {
                ProcessEvent(eventEnvelope, id, existingViews, dbSet);
            }
        }, token);


    private void ProcessEvent(IEventEnvelope eventEnvelope, TId? id, Dictionary<TId, TView> existingViews,
        DbSet<TView> dbSet)
    {
        var current = id.HasValue && existingViews.TryGetValue(id.Value, out var existing) ? existing : null;

        var result = Apply(current ?? GetDefault(), eventEnvelope);

        if (result == null)
        {
            if (current != null)
                dbSet.Remove(current);

            return;
        }

        DbContext.AddOrUpdate(result);

        if (current == null)
            existingViews.Add(viewId(result), result);
    }

    protected virtual TView GetDefault() =>
        ObjectFactory<TView>.GetDefaultOrUninitialized();

    private Expression<Func<TView, bool>> BuildContainsExpression(List<TId> ids) =>
        GetContainsExpression(ids);

    private async Task<Dictionary<TId, TView>>
        GetExistingViews(DbSet<TView> dbSet, List<TId> ids, CancellationToken ct) =>
        Include != null
            ? await dbSet.Include(Include).Where(BuildContainsExpression(ids)).ToDictionaryAsync(viewId, ct)
            : await dbSet.Where(BuildContainsExpression(ids)).ToDictionaryAsync(viewId, ct);

    private Expression<Func<TView, bool>> GetContainsExpression(List<TId> ids)
    {
        var parameter = viewIdExpression.Parameters.Single();
        var body = Expression.Call(
            Expression.Constant(ids),
            ((Func<TId, bool>)ids.Contains).Method,
            viewIdExpression.Body
        );

        return Expression.Lambda<Func<TView, bool>>(body, parameter);
    }
}
