using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Events;
using ECommerce.Core.Projections;
using ECommerce.Storage;

namespace ECommerce.ShoppingCarts.GettingCarts
{
    public record ShoppingCartShortInfo
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public int TotalItemsCount { get; set; }
        public ShoppingCartStatus Status { get; set; }
    }

    public class ShoppingCartShortInfoProjection:
        EntityFrameworkProjection<ShoppingCartShortInfo>,
        IEventHandler<ShoppingCartInitialized>,
        IEventHandler<ShoppingCartConfirmed>
    {
        public ShoppingCartShortInfoProjection(ECommerceDBContext dbContext)
            : base(dbContext)
        {
        }

        public Task Handle(ShoppingCartInitialized @event, CancellationToken ct)
        {
            var (shoppingCartId, clientId, shoppingCartStatus) = @event;

            return Add(
                new ShoppingCartShortInfo
                {
                    Id = shoppingCartId,
                    ClientId = clientId,
                    TotalItemsCount = 0,
                    Status = shoppingCartStatus
                }, ct);
        }

        public Task Handle(ShoppingCartConfirmed @event, CancellationToken ct)
        {
            var (cartId, _) = @event;

            return Update(cartId, entity =>
            {
                entity.Status = ShoppingCartStatus.Confirmed;
            }, ct);
        }
    }
}
