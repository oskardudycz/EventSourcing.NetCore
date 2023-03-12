using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Marten;
using Marten.Events;
using Marten.Events.Projections;

namespace MartenMeetsElastic.Projections;

public abstract class ElasticsearchProjection<TDocument>: IProjection
{
    private readonly ElasticsearchClient elasticsearchClient;

    private readonly Dictionary<Type, Action<object>> handlers = new();

    public ElasticsearchProjection(ElasticsearchClient elasticsearchClient) =>
        this.elasticsearchClient = elasticsearchClient;

    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams) =>
        throw new NotImplementedException("We don't want to do 2PC, aye?");

    public Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<StreamAction> streamActions, CancellationToken cancellation)
    {
        var events = streamActions.SelectMany(streamAction => streamAction.Events)
            .Where(@event => handlers.ContainsKey(@event.EventType))
            .ToList();

        var documents = elasticsearchClient.Search<TDocument>(new SearchRequest()
        {
            Query = new TermQuery("id", )
        })

        throw new NotImplementedException();
    }

    protected abstract void Configure();

    protected virtual void SetupMapping(ElasticsearchClient elasticsearchClient){}

    protected void Projects<TEvent>(Action<TEvent> action)
    {
        handlers.Add(typeof(TEvent), @event => action(elasticsearchClient, (TEvent) @event));
    }



    public bool EnableDocumentTrackingDuringRebuilds { get; set; }
}
