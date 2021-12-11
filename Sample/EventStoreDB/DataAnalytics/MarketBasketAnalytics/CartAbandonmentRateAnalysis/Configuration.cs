using System;
using System.Threading;
using System.Threading.Tasks;
using DataAnalytics.Core.ElasticSearch;
using DataAnalytics.Core.Entities;
using DataAnalytics.Core.Events;
using EventStore.Client;
using MarketBasketAnalytics.Carts;
using Microsoft.Extensions.DependencyInjection;
using Nest;

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
                        eventStore.AggregateStream,
                        shoppingCartAbandoned,
                        ct
                    );

                    await eventStore.AppendToNewStream(
                        CartAbandonmentRate.ToStreamId(shoppingCartAbandoned.ShoppingCartId),
                        @event,
                        ct
                    );
                })
                .AddEventHandler<ShoppingCartConfirmed>(async (sp, shoppingCartConfirmed, ct) =>
                {
                    var elastic = sp.GetRequiredService<IElasticClient>();

                    var summaryId = CartAbandonmentRatesSummary.SummaryId;

                    var summary = await CartAbandonmentRatesSummary.Handle(
                        token => elastic.Find<CartAbandonmentRatesSummary>(summaryId, token),
                        shoppingCartConfirmed,
                        ct
                    );

                    await elastic.Upsert(summaryId, summary, ct);
                })
                .AddEventHandler<CartAbandonmentRateCalculated>(async (sp, shoppingCartAbandoned, ct) =>
                {
                    var elastic = sp.GetRequiredService<IElasticClient>();

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
