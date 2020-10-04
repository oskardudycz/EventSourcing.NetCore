using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Aggregates;
using Core.Events;
using Core.Storage;

namespace MeetingsSearch.Storage
{
    public class ElasticSearchRepository<T>: IRepository<T> where T : class, IAggregate, new()
    {
        private readonly Nest.IElasticClient elasticClient;
        private readonly IEventBus eventBus;

        public ElasticSearchRepository(
            Nest.IElasticClient elasticClient,
            IEventBus eventBus
        )
        {
            this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
            this.eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        public async Task<T> Find(Guid id, CancellationToken cancellationToken)
        {
            var response = await elasticClient.GetAsync<T>(id, ct: cancellationToken);
            return response.Source;
        }

        public Task Add(T aggregate, CancellationToken cancellationToken)
        {
            return elasticClient.IndexAsync(aggregate, i => i.Id(aggregate.Id), cancellationToken);
        }

        public Task Update(T aggregate, CancellationToken cancellationToken)
        {
            return elasticClient.UpdateAsync<T>(aggregate.Id, i => i.Doc(aggregate), cancellationToken);
        }

        public Task Delete(T aggregate, CancellationToken cancellationToken)
        {
            return elasticClient.DeleteAsync<T>(aggregate.Id, ct: cancellationToken);
        }
    }
}
