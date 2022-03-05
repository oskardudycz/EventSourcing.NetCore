using Core.ElasticSearch.Indices;
using Core.Events;
using Core.Projections;
using Elasticsearch.Net;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Core.ElasticSearch.Projections;

public class ElasticSearchProjection<TEvent, TView> : IEventHandler<EventEnvelope<TEvent>>
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

    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var id = getId(eventEnvelope.Data);
        var indexName = IndexNameMapper.ToIndexName<TView>();

        var entity = (await elasticClient.GetAsync<TView>(id, i => i.Index(indexName), ct))?.Source ??
                     (TView) Activator.CreateInstance(typeof(TView), true)!;

        entity.When(eventEnvelope);

        await elasticClient.IndexAsync(
            entity,
            i => i.Index(indexName).Id(id).VersionType(VersionType.External).Version((long)eventEnvelope.Metadata.StreamPosition),
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
        services.AddTransient<IEventHandler<EventEnvelope<TEvent>>>(sp =>
        {
            var session = sp.GetRequiredService<IElasticClient>();

            return new ElasticSearchProjection<TEvent, TView>(session, getId);
        });

        return services;
    }
}
