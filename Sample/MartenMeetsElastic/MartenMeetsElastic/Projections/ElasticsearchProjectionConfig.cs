using Elastic.Clients.Elasticsearch;
using Marten.Events.Projections;

namespace MartenMeetsElastic.Projections;

public static class ElasticsearchProjectionConfig
{
    public static void Add<TElasticsearchProjection>(
        this ProjectionOptions projectionOptions,
        ElasticsearchClient client
    ) where TElasticsearchProjection : ElasticsearchProjection, new() =>
        projectionOptions.Add(
            new TElasticsearchProjection { ElasticsearchClient = client },
            ProjectionLifecycle.Async
        );
}
