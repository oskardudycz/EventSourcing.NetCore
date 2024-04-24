using Core.ElasticSearch.Indices;
using Core.Reflection;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Marten;
using Marten.Events;
using Marten.Events.Projections;

namespace MartenMeetsElastic.Projections;

public abstract class ElasticsearchProjection: IProjection
{
    protected abstract string IndexName { get; }
    public ElasticsearchClient ElasticsearchClient { private get; init; } = default!;

    private readonly HashSet<Type> handledEventTypes = new();

    protected void Projects<TEvent>() =>
        handledEventTypes.Add(typeof(TEvent));

    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams) =>
        throw new NotImplementedException("We don't want to do 2PC, aye?");

    public async Task ApplyAsync(
        IDocumentOperations operations,
        IReadOnlyList<StreamAction> streamActions,
        CancellationToken cancellation
    )
    {
        var existsResponse = await ElasticsearchClient.Indices.ExistsAsync(IndexName, cancellation);
        if (!existsResponse.Exists)
            await SetupMapping(ElasticsearchClient);

        var events = streamActions.SelectMany(streamAction => streamAction.Events)
            .Where(@event => handledEventTypes.Contains(@event.EventType))
            .ToArray();

        await ApplyAsync(ElasticsearchClient, events);
    }

    protected virtual Task ApplyAsync(ElasticsearchClient client, IEvent[] events) =>
        ApplyAsync(
            client,
            events.Where(@event => handledEventTypes.Contains(@event.EventType)).Select(@event => @event.Data).ToArray()
        );

    protected virtual Task ApplyAsync(ElasticsearchClient client, object[] events) =>
        Task.CompletedTask;


    protected virtual Task SetupMapping(ElasticsearchClient client) =>
        client.Indices.CreateAsync(IndexName);


    public bool EnableDocumentTrackingDuringRebuilds { get; set; }
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

protected override async Task ApplyAsync(ElasticsearchClient client, object[] events)
{
    var ids = events.Select(GetDocumentId).ToList();

    var searchResponse = await client.SearchAsync<TDocument>(s => s
        .Index(IndexName)
        .Query(q => q.Ids(new IdsQuery { Values = new Ids(ids) }))
    );

    var existingDocuments = searchResponse.Documents.ToDictionary(ks => getDocumentId(ks), vs => vs);

    var updatedDocuments = events.Select((@event, i) =>
        Apply(existingDocuments.GetValueOrDefault(ids[i], GetDefault(@event)), @event)
    ).ToList();

    var bulkAll = client.BulkAll(updatedDocuments, SetBulkOptions);

    bulkAll.Wait(TimeSpan.FromSeconds(5), _ => Console.WriteLine("Data indexed"));
}
protected virtual TDocument GetDefault(object @event) =>
    ObjectFactory<TDocument>.GetDefaultOrUninitialized();

private TDocument Apply(TDocument document, object @event) =>
    projectors[@event.GetType()].Apply(document, @event);

protected virtual void SetBulkOptions(BulkAllRequestDescriptor<TDocument> options) =>
    options.Index(IndexName);

    protected override Task SetupMapping(ElasticsearchClient client) =>
        client.Indices.CreateAsync<TDocument>(IndexName);

    private string GetDocumentId(object @event) =>
        projectors[@event.GetType()].GetId(@event);

}
