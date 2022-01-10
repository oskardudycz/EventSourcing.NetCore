// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Elasticsearch.Net;
// using Marten.Events;
// using Nest;
//
// namespace Marten.Integration.Tests.Integration;
//
// public class ElasticSearchProjection
// {
//
// }
//
// public class ElasticSearchProjection<TEvent, TView> : IMartenEventsConsumer
//     where TView : class
//     where TEvent : notnull
// {
//     private readonly IElasticClient elasticClient;
//     private readonly Func<TEvent, string> getId;
//
//     public ElasticSearchProjection(
//         IElasticClient elasticClient,
//         Func<TEvent, string> getId
//     )
//     {
//         this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
//         this.getId = getId ?? throw new ArgumentNullException(nameof(getId));
//     }
//
//     public async Task Handle(StreamEvent<TEvent> @event, CancellationToken ct)
//     {
//         var id = getId(@event.Data);
//         var indexName = IndexNameMapper.ToIndexName<TView>();
//
//         var entity = (await elasticClient.GetAsync<TView>(id, i => i.Index(indexName), ct))?.Source ??
//                      (TView) Activator.CreateInstance(typeof(TView), true)!;
//
//         entity.When(@event.Data);
//
//         await elasticClient.IndexAsync(
//             entity,
//             i => i.Index(indexName).Id(id).VersionType(VersionType.External).Version((long)@event.Metadata.StreamRevision),
//             ct
//         );
//     }
//
//     public Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions)
//     {
//         foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
//         {
//         }
//     }
// }
