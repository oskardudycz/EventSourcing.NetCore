// using System.Linq;
// using System.Threading;
// using System.Threading.Tasks;
// using Ardalis.GuardClauses;
// using Carts.Carts.Events;
// using Carts.Carts.Events.External;
// using Core.Events;
// using Core.Exceptions;
// using Marten;
//
// namespace Carts.Carts
// {
//     TODO: Add example using EventStoreDB projection instead of that
//     internal class CartEventHandler : IEventHandler<CartConfirmed>
//     {
//         private readonly IQuerySession querySession;
//         private readonly IEventBus eventBus;
//
//         public CartEventHandler(
//             IQuerySession querySession,
//             IEventBus eventBus
//         )
//         {
//             Guard.Against.Null(querySession, nameof(querySession));
//             Guard.Against.Null(eventBus, nameof(eventBus));
//
//             this.querySession = querySession;
//             this.eventBus = eventBus;
//         }
//
//         public async Task Handle(CartConfirmed @event, CancellationToken cancellationToken)
//         {
//             var cart = await querySession.LoadAsync<Cart>(@event.CartId, cancellationToken)
//                 ?? throw  AggregateNotFoundException.For<Cart>(@event.CartId);
//
//             var externalEvent = CartFinalized.Create(
//                 @event.CartId,
//                 cart.ClientId,
//                 cart.ProductItems.ToList(),
//                 cart.TotalPrice,
//                 @event.ConfirmedAt
//             );
//
//             await eventBus.Publish(externalEvent);
//         }
//     }
// }
