using System;
using System.Threading;
using System.Threading.Tasks;
using ECommerce.Core.Events;
using ECommerce.Core.Projections;
using ECommerce.Storage;

namespace ECommerce.ShoppingCarts.GettingCartById
{
    public class ShoppingCartDetails
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public ShoppingCartStatus Status { get; set; }

        // public IList<PricedProductItem> ProductItems { get; set; } = default!;

        // public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);
        public int Version { get; set; }
    }

    public class ShoppingCartDetailsProjection :
        EntityFrameworkProjection<ShoppingCartDetails>,
        IEventHandler<ShoppingCartInitialized>,
        IEventHandler<ShoppingCartConfirmed>
    {
        public ShoppingCartDetailsProjection(ECommerceDBContext dbContext) : base(dbContext)
        {
        }

        public Task Handle(ShoppingCartInitialized @event, CancellationToken ct)
        {
            var (shoppingCartId, clientId, shoppingCartStatus) = @event;

            return Add(
                new ShoppingCartDetails
                {
                    Id = shoppingCartId,
                    ClientId = clientId,
                    Status = shoppingCartStatus,
                    Version = 1
                }, ct);
        }

        public Task Handle(ShoppingCartConfirmed @event, CancellationToken ct)
        {
            var (cartId, _) = @event;

            return Update(cartId, entity =>
            {
                entity.Status = ShoppingCartStatus.Confirmed;
                entity.Version++;
            }, ct);
        }
    }
}
