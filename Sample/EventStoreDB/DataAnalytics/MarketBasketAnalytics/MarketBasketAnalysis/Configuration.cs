using Core.Events;
using Core.EventStoreDB.Events;
using Core.EventStoreDB.Serialization;
using EventStore.Client;
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
                        (evolve, streamName, t) => eventStore.Find(evolve, streamName, t),
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
