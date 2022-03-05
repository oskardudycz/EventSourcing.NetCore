using Core.ElasticSearch.Indices;
using Core.Events;
using Nest;
using IAggregate = Core.Aggregates.IAggregate;

namespace Core.ElasticSearch.Repository;


public interface IElasticSearchRepository<T> where T : class, IAggregate, new()
{
    Task<T?> Find(Guid id, CancellationToken cancellationToken);
    Task Add(T aggregate, CancellationToken cancellationToken);
    Task Update(T aggregate, CancellationToken cancellationToken);
    Task Delete(T aggregate, CancellationToken cancellationToken);
}

public class ElasticSearchRepository<T>: IElasticSearchRepository<T> where T : class, IAggregate, new()
{
    private readonly IElasticClient elasticClient;

    public ElasticSearchRepository(
        IElasticClient elasticClient
    )
    {
        this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
    }

    public async Task<T?> Find(Guid id, CancellationToken cancellationToken)
    {
        var response = await elasticClient.GetAsync<T>(id, ct: cancellationToken);
        return response?.Source;
    }

    public Task Add(T aggregate, CancellationToken cancellationToken)
    {
        return elasticClient.IndexAsync(aggregate, i => i.Id(aggregate.Id).Index(IndexNameMapper.ToIndexName<T>()), cancellationToken);
    }

    public Task Update(T aggregate, CancellationToken cancellationToken)
    {
        return elasticClient.UpdateAsync<T>(aggregate.Id, i => i.Doc(aggregate).Index(IndexNameMapper.ToIndexName<T>()), cancellationToken);
    }

    public Task Delete(T aggregate, CancellationToken cancellationToken)
    {
        return elasticClient.DeleteAsync<T>(aggregate.Id, ct: cancellationToken);
    }
}
