using System.Collections.Concurrent;
using Elastic.Clients.Elasticsearch;

namespace Core.ElasticSearch.Repository;

public static class ElasticSearchRepository
{
    public static async Task<T?> Find<T>(this ElasticsearchClient elasticClient, string id, CancellationToken ct)
        where T : class =>
        (await elasticClient.GetAsync<T>(id, cancellationToken: ct).ConfigureAwait(false))?.Source;

    public static async Task Upsert<T>(this ElasticsearchClient elasticClient, string id, T entity,
        CancellationToken ct)
        where T : class =>
        await elasticClient.UpdateAsync<T, object>(ToIndexName<T>(), id,
            u => u.Doc(entity).Upsert(entity),
            ct
        ).ConfigureAwait(false);

    private static readonly ConcurrentDictionary<Type, string> TypeNameMap = new();

    private static string ToIndexName<TIndex>()
    {
        var indexType = typeof(TIndex);
        return TypeNameMap.GetOrAdd(indexType, _ =>
        {
            var modulePrefix = indexType.Namespace!.Split('.').First();
            return $"{modulePrefix}-{indexType.Name}".ToLower();
        });
    }
}
