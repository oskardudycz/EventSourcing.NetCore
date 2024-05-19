using Core.Events;
using Core.Reflection;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace Core.EntityFramework;

public class EntityFrameworkProjection<TDbContext>(TDbContext dbContext, IAsyncPolicy? retryPolicy = null)
    : IEventBatchHandler
    where TDbContext : DbContext
{
    protected readonly TDbContext DBContext = dbContext;
    protected IAsyncPolicy RetryPolicy { get; } = retryPolicy ?? Policy.NoOpAsync();
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

public abstract class EntityFrameworkProjection<TDocument, TDbContext>(
    TDbContext dbContext,
    IAsyncPolicy retryPolicy
): EntityFrameworkProjection<TDbContext>(dbContext, retryPolicy)
    where TDocument : class
    where TDbContext : DbContext
{
    private record ProjectEvent(
        Func<object, string> GetId,
        Func<TDocument, object, TDocument> Apply
    );

    private readonly Dictionary<Type, ProjectEvent> projectors = new();
    private Func<TDocument, object> getDocumentId = default!;

    protected void Projects<TEvent>(
        Func<TEvent, string> getId,
        Func<TDocument, TEvent, TDocument> apply
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

    protected void DocumentId(Func<TDocument, object> documentId) =>
        getDocumentId = documentId;

    protected override Task ApplyAsync(object[] events, CancellationToken token) =>
        RetryPolicy.ExecuteAsync(async ct =>
        {
            var ids = events.Select(GetDocumentId).ToList();

            var entities = await DBContext.Set<TDocument>()
                .Where(x => ids.Contains(getDocumentId(x)))
                .ToListAsync(cancellationToken: ct);

            var existingDocuments = entities.ToDictionary(ks => getDocumentId(ks), vs => vs);

            for(var i = 0; i < events.Length; i++)
            {
                Apply(existingDocuments.GetValueOrDefault(ids[i], GetDefault(events[i])), events[i]);
            }
        }, token);

    protected virtual TDocument GetDefault(object @event) =>
        ObjectFactory<TDocument>.GetDefaultOrUninitialized();

    private TDocument Apply(TDocument document, object @event) =>
        projectors[@event.GetType()].Apply(document, @event);

    private object GetDocumentId(object @event) =>
        projectors[@event.GetType()].GetId(@event);
}
