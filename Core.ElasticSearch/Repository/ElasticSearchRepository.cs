using System;
using System.Threading;
using System.Threading.Tasks;
using Core.ElasticSearch.Indices;
using Core.Events;
using Nest;
using IAggregate = Core.Aggregates.IAggregate;

namespace Core.ElasticSearch.Repository;

public class ElasticSearchRepository<T>: Repositories.IRepository<T> where T : class, IAggregate, new()
{
    private readonly IElasticClient elasticClient;
    private readonly IEventBus eventBus;

    public ElasticSearchRepository(
        IElasticClient elasticClient,
        IEventBus eventBus
    )
    {
        this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
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