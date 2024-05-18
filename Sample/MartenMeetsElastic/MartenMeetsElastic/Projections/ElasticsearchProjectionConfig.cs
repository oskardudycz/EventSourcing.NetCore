using Elastic.Clients.Elasticsearch;
using Marten.Events;

namespace MartenMeetsElastic.Projections;

public static class ElasticsearchProjectionConfig
{
    public static void AddElasticsearchProjection<TElasticsearchProjection>(
        this IEventStoreOptions options,
        ElasticsearchClient client
    ) where TElasticsearchProjection : ElasticsearchProjection, new() =>
        options.Subscribe(
            new TElasticsearchProjection { ElasticsearchClient = client }
        );
}
