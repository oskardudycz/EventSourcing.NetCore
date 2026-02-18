using Core.ElasticSearch.Indices;
using Core.Reflection;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Marten;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten.Subscriptions;
using Polly;

namespace MartenMeetsElastic.Projections;

public abstract class ElasticsearchProjection: SubscriptionBase
{
    protected abstract string IndexName { get; }
    public ElasticsearchClient ElasticsearchClient { private get; init; } = default!;
    public IAsyncPolicy RetryPolicy { protected get; init; } = Policy.NoOpAsync();

    private readonly HashSet<Type> handledEventTypes = [];

    protected void Projects<TEvent>() =>
        handledEventTypes.Add(typeof(TEvent));

    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange eventRange,
        ISubscriptionController subscriptionController,
        IDocumentOperations operations,
        CancellationToken ct
    )
    {
        try
        {
            //TODO: Add poly!
            var existsResponse = await ElasticsearchClient.Indices.ExistsAsync(IndexName, ct);
            if (!existsResponse.Exists)
                await SetupMapping(ElasticsearchClient);

            var events = eventRange.Events
                .Where(@event => handledEventTypes.Contains(@event.EventType))
                .ToArray();

            await ApplyAsync(ElasticsearchClient, events, ct);
        }
        catch (Exception exc)
        {
            await subscriptionController.ReportCriticalFailureAsync(exc);
        }

        return NullChangeListener.Instance;
    }

    protected virtual Task ApplyAsync(ElasticsearchClient client, IEvent[] events, CancellationToken ct) =>
        ApplyAsync(
            client,
            events.Where(@event => handledEventTypes.Contains(@event.EventType)).Select(@event => @event.Data)
                .ToArray(),
            ct
        );

    protected virtual Task ApplyAsync(ElasticsearchClient client, object[] events, CancellationToken ct) =>
        Task.CompletedTask;


    protected virtual Task SetupMapping(ElasticsearchClient client) =>
        client.Indices.CreateAsync(IndexName);
}

public abstract class ElasticsearchProjection<TDocument>:
    ElasticsearchProjection where TDocument : class
{
    private record ProjectEvent(
        Func<object, string> GetId,
        Func<TDocument, object, TDocument> Apply
    );

    protected override string IndexName => IndexNameMapper.ToIndexName<TDocument>();

    private readonly Dictionary<Type, ProjectEvent> projectors = new();
    private Func<TDocument, string> getDocumentId = default!;

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

    private new void Projects<TEvent>() =>
        base.Projects<TEvent>();

    protected void DocumentId(Func<TDocument, string> documentId) =>
        getDocumentId = documentId;

    protected override Task ApplyAsync(ElasticsearchClient client, object[] events, CancellationToken token) =>
        RetryPolicy.ExecuteAsync(async ct =>
        {
            var ids = events.Select(GetDocumentId).ToList();

            var searchResponse = await client.SearchAsync<TDocument>(s => s
                .Index(IndexName)
                .Query(q => q.Ids(new IdsQuery { Values = new Ids(ids) })), ct);

            var existingDocuments = searchResponse.Documents.ToDictionary(ks => getDocumentId(ks), vs => vs);

            var updatedDocuments = events.Select((@event, i) =>
                Apply(existingDocuments.GetValueOrDefault(ids[i], GetDefault(@event)), @event)
            ).ToList();

            var bulkAll = client.BulkAll(updatedDocuments, SetBulkOptions, ct);

            bulkAll.Wait(TimeSpan.FromSeconds(5), _ => Console.WriteLine("Data indexed"));
        }, token);

    protected virtual TDocument GetDefault(object @event) =>
        ObjectFactory<TDocument>.GetDefaultOrUninitialized();

    private TDocument Apply(TDocument document, object @event) =>
        projectors[@event.GetType()].Apply(document, @event);

    protected virtual void SetBulkOptions(BulkAllRequestDescriptor<TDocument> options) =>
        options.Index(IndexName)
            // .ContinueAfterDroppedDocuments()
            // .DroppedDocumentCallback((r, o) =>
            // {
            //     Console.WriteLine($"{r} {o}");
            // })
            .BackOffTime(TimeSpan.FromMilliseconds(1))
            .RefreshOnCompleted();

    protected override Task SetupMapping(ElasticsearchClient client) =>
        client.Indices.CreateAsync<TDocument>(IndexName);

    private string GetDocumentId(object @event) =>
        projectors[@event.GetType()].GetId(@event);
}
