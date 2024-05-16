using Core.ElasticSearch.Indices;
using Elastic.Clients.Elasticsearch;
using IAggregate = Core.Aggregates.IAggregate;

namespace Core.ElasticSearch.Repository;


public interface IElasticSearchRepository<T> where T : class, IAggregate
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task Add(Guid id, T aggregate, CancellationToken cancellationToken);
    Task Update(Guid id, T aggregate, CancellationToken cancellationToken);
    Task Delete(Guid id, T aggregate, CancellationToken cancellationToken);
}

public class ElasticSearchRepository<T>(ElasticsearchClient elasticClient): IElasticSearchRepository<T>
    where T : class, IAggregate
{
    private readonly ElasticsearchClient elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));

    public async Task<T?> Find(Guid id, CancellationToken cancellationToken)
    {
        var response = await elasticClient.GetAsync<T>(id, cancellationToken).ConfigureAwait(false);
        return response?.Source;
    }

    public Task Add(Guid id, T aggregate, CancellationToken cancellationToken)
    {
        return elasticClient.IndexAsync(aggregate, i => i.Id(id).Index(IndexNameMapper.ToIndexName<T>()), cancellationToken);
    }

    public Task Update(Guid id, T aggregate, CancellationToken cancellationToken)
    {
        return elasticClient.UpdateAsync<T, object>(IndexNameMapper.ToIndexName<T>(), id, i => i.Doc(aggregate), cancellationToken);
    }

    public Task Delete(Guid id, T aggregate, CancellationToken cancellationToken)
    {
        return elasticClient.DeleteAsync<T>(id, cancellationToken);
    }
}
