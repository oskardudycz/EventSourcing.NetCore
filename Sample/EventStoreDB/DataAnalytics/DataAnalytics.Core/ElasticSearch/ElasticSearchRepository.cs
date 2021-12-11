using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace DataAnalytics.Core.ElasticSearch
{
    public static class ElasticSearchRepository
    {

        public static async Task<T?> Find<T>(this IElasticClient elasticClient, string id, CancellationToken ct)
            where T: class =>
            (await elasticClient.GetAsync<T>(id, ct: ct))?.Source;

        public static async Task Upsert<T>(this IElasticClient elasticClient, string id, T entity, CancellationToken ct)
            where T: class =>
            await elasticClient.UpdateAsync<T>(id,
                u => u.Doc(entity).Upsert(entity).Index(ToIndexName<T>()),
                ct
            );

        private static readonly ConcurrentDictionary<Type, string> TypeNameMap = new();

        private static string ToIndexName<TIndex>()
        {
            var indexType = typeof(TIndex);
            return TypeNameMap.GetOrAdd(indexType, _ =>
            {
                var modulePrefix = indexType.Namespace!.Split(".").First();
                return $"{modulePrefix}-{indexType.Name}".ToLower();
            });
        }
    }
}

