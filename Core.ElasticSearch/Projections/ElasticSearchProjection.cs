using Core.ElasticSearch.Indices;
using Core.Events;
using Core.Events.NoMediator;
using Core.Projections;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Core.ElasticSearch.Projections;

public class ElasticSearchProjection<TEvent, TView> : INoMediatorEventHandler<StreamEvent<TEvent>>
    where TView : class, IProjection
    where TEvent : IEvent
{
    private readonly IElasticClient elasticClient;
    private readonly Func<TEvent, string> getId;

    public ElasticSearchProjection(
        IElasticClient elasticClient,
        Func<TEvent, string> getId
    )
    {
        this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        this.getId = getId ?? throw new ArgumentNullException(nameof(getId));
    }

    public async Task Handle(StreamEvent<TEvent> @event, CancellationToken ct)
    {
        var id = getId(@event.Data);
        var indexName = IndexNameMapper.ToIndexName<TView>();

        var entity = (await elasticClient.GetAsync<TView>(id, i => i.Index(indexName), ct))?.Source ??
                     (TView) Activator.CreateInstance(typeof(TView), true)!;

        entity.When(@event);

        await elasticClient.IndexAsync(
            entity,
            i => i.Index(indexName).Id(id).VersionType(VersionType.External).Version((long)@event.Metadata.StreamRevision),
            ct
        );
    }
}

public static class ElasticSearchProjectionConfig
{
    public static IServiceCollection Project<TEvent, TView>(this IServiceCollection services,
        Func<TEvent, string> getId)
        where TView : class, IProjection
        where TEvent : IEvent
    {
        services.AddTransient<INoMediatorEventHandler<StreamEvent<TEvent>>>(sp =>
        {
            var session = sp.GetRequiredService<IElasticClient>();

            return new ElasticSearchProjection<TEvent, TView>(session, getId);
        });

        return services;
    }
}
