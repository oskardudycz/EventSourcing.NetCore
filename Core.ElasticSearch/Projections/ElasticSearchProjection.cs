using Core.ElasticSearch.Indices;
using Core.Events;
using Core.Projections;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;

namespace Core.ElasticSearch.Projections;

public class ElasticSearchProjection<TEvent, TView>(
    ElasticsearchClient elasticClient,
    Func<TEvent, string> getId)
    : IEventHandler<EventEnvelope<TEvent>>
    where TView : class, IProjection
    where TEvent : notnull
{
    private readonly ElasticsearchClient elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    private readonly Func<TEvent, string> getId = getId ?? throw new ArgumentNullException(nameof(getId));

    public async Task Handle(EventEnvelope<TEvent> eventEnvelope, CancellationToken ct)
    {
        var id = getId(eventEnvelope.Data);
        var indexName = IndexNameMapper.ToIndexName<TView>();

        var entity = (await elasticClient.GetAsync<TView>(id, i => i.Index(indexName), ct).ConfigureAwait(false))?.Source ??
                     (TView) Activator.CreateInstance(typeof(TView), true)!;

        entity.Evolve(eventEnvelope);

        await elasticClient.IndexAsync(
            entity,
            i => i.Index(indexName).Id(id).VersionType(VersionType.External).Version((long)eventEnvelope.Metadata.StreamPosition),
            ct
        ).ConfigureAwait(false);
    }
}

public static class ElasticSearchProjectionConfig
{
    public static IServiceCollection Project<TEvent, TView>(this IServiceCollection services,
        Func<TEvent, string> getId)
        where TView : class, IProjection
        where TEvent : notnull
    {
        services.AddTransient<IEventHandler<EventEnvelope<TEvent>>>(sp =>
        {
            var session = sp.GetRequiredService<ElasticsearchClient>();

            return new ElasticSearchProjection<TEvent, TView>(session, getId);
        });

        return services;
    }
}
