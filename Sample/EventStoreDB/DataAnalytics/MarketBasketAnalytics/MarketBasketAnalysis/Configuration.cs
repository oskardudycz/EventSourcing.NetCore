using System;
using System.Linq;
using DataAnalytics.Core.Entities;
using DataAnalytics.Core.Events;
using DataAnalytics.Core.Serialisation;
using EventStore.Client;
using MarketBasketAnalytics.CartAbandonmentRateAnalysis;
using MarketBasketAnalytics.Carts;
using Microsoft.Extensions.DependencyInjection;

namespace MarketBasketAnalytics.MarketBasketAnalysis
{
    public static class Configuration
    {
        public static IServiceCollection AddMarketBasketAnalysis(this IServiceCollection services) =>
            services
                .AddEventHandler<ShoppingCartConfirmed>(async (sp, shoppingCartConfirmed, ct) =>
                {
                    var eventStore = sp.GetRequiredService<EventStoreClient>();

                    var events = await CartProductItemsMatching.Handle(
                        eventStore.AggregateStream,
                        shoppingCartConfirmed,
                        ct
                    );

                    await eventStore.AppendToStreamAsync(
                        CartProductItemsMatching.ToStreamId(shoppingCartConfirmed.ShoppingCartId),
                        StreamState.NoStream,
                        events.Select(@event => @event.ToJsonEventData()),
                        cancellationToken: ct
                    );
                })
                .AddEventHandler<CartProductItemsMatched>(async (sp, cartProductItemsMatched, ct) =>
                {
                    var eventStore = sp.GetRequiredService<EventStoreClient>();

                    var @event = await ProductRelationships.Handle(
                        (id, token) => eventStore.ReadLastEvent<ProductRelationshipsCalculated>(id, token),
                        cartProductItemsMatched,
                        ct
                    );

                    await eventStore.AppendToStreamWithSingleEvent(
                        ProductRelationships.ToStreamId(cartProductItemsMatched.ProductId),
                        @event,
                        ct
                    );
                });
    }
}
