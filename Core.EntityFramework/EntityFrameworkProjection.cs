using System.Linq.Expressions;
using Core.Events;
using Core.Reflection;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.EntityFramework;

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
    where TId: struct
    where TDbContext : DbContext
{
    private record ProjectEvent(
        Func<object, TId?> GetId,
        Func<TView, object, TView?> Apply
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
        Func<TEvent, TView> apply
    )
    {
        projectors.Add(
            typeof(TEvent),
            new ProjectEvent(
                _ => null,
                (_, @event) => apply((TEvent)@event)
            )
        );
        Projects<TEvent>();
    }


    public void Deletes<TEvent>(
        Func<TEvent, TId> getId
    )
    {
        projectors.Add(
            typeof(TEvent),
            new ProjectEvent(
                @event => getId((TEvent)@event),
                (_, _) => null
            )
        );
        Projects<TEvent>();
    }

    public void Projects<TEvent>(
        Func<TEvent, TId> getId,
        Func<TView, TEvent, TView> apply
    )
    {
        projectors.Add(
            typeof(TEvent),
            new ProjectEvent(
                @event => getId((TEvent)@event),
                (document, @event) => apply(document, (TEvent)@event)
            )
        );
        Projects<TEvent>();
    }

    protected override Task ApplyAsync(object[] events, CancellationToken token) =>
        RetryPolicy.ExecuteAsync(async ct =>
        {
            var dbSet = DbContext.Set<TView>();

            var ids = events.Select(GetViewId).ToList();

            var idPredicate = GetContainsExpression(ids.Where(e => e.HasValue).Cast<TId>().ToList());

            var existingViews = await dbSet
                .Where(idPredicate)
                .ToDictionaryAsync(viewId, ct);

            for (var i = 0; i < events.Length; i++)
            {
                var id = ids[i];

                var current = id.HasValue && existingViews.TryGetValue(id.Value, out var existing) ? existing : null;

                var result = Apply(current ?? GetDefault(events[i]), events[i]);

                if (result == null)
                {
                    if (current != null)
                        dbSet.Remove(current);
                }
                else if (current == null)
                    dbSet.Add(result);
                else
                    dbSet.Update(result);
            }
        }, token);

    protected virtual TView GetDefault(object @event) =>
        ObjectFactory<TView>.GetDefaultOrUninitialized();

    private TView? Apply(TView document, object @event) =>
        projectors[@event.GetType()].Apply(document, @event);

    private TId? GetViewId(object @event) =>
        projectors[@event.GetType()].GetId(@event);

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
