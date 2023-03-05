using Core.ElasticSearch.Repository;
using Core.Events;
using Core.EventStoreDB.Events;
using Elastic.Clients.Elasticsearch;
using EventStore.Client;
using MarketBasketAnalytics.Carts;
using Microsoft.Extensions.DependencyInjection;

namespace MarketBasketAnalytics.CartAbandonmentRateAnalysis
{
    public static class Configuration
    {
        public static IServiceCollection AddCartAbandonmentRateAnalysis(this IServiceCollection services) =>
            services
                .AddEventHandler<ShoppingCartAbandoned>(async (sp, shoppingCartAbandoned, ct) =>
                {
                    var eventStore = sp.GetRequiredService<EventStoreClient>();

                    var @event = await CartAbandonmentRate.Handle(
                        (evolve, streamName, t) => eventStore.Find(evolve, streamName, t),
                        shoppingCartAbandoned,
                        ct
                    );

                    await eventStore.Append(
                        CartAbandonmentRate.ToStreamId(shoppingCartAbandoned.ShoppingCartId),
                        @event,
                        ct
                    );
                })
                .AddEventHandler<ShoppingCartConfirmed>(async (sp, shoppingCartConfirmed, ct) =>
                {
                    var elastic = sp.GetRequiredService<ElasticsearchClient>();

                    var summaryId = CartAbandonmentRatesSummary.SummaryId;

                    var summary = await CartAbandonmentRatesSummary.Handle(
                        async token => (await elastic.GetAsync<CartAbandonmentRatesSummary>(summaryId, cancellationToken: token)).Source,
                        shoppingCartConfirmed,
                        ct
                    );

                    await elastic.Upsert(summaryId, summary, ct);
                })
                .AddEventHandler<CartAbandonmentRateCalculated>(async (sp, shoppingCartAbandoned, ct) =>
                {
                    var elastic = sp.GetRequiredService<ElasticsearchClient>();

                    var summaryId = CartAbandonmentRatesSummary.SummaryId;

                    var summary = await CartAbandonmentRatesSummary.Handle(
                        token => elastic.Find<CartAbandonmentRatesSummary>(summaryId, token),
                        shoppingCartAbandoned,
                        ct
                    );

                    await elastic.Upsert(summaryId, summary, ct);
                });

    }
}
