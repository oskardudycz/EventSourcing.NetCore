using ECommerce.ShoppingCarts.ProductItems;

namespace ECommerce.ShoppingCarts;

public record ShoppingCartOpened(
    Guid ShoppingCartId,
    Guid ClientId
);

public record ProductItemAddedToShoppingCart(
    Guid ShoppingCartId,
    PricedProductItem ProductItem
);

public record ProductItemRemovedFromShoppingCart(
    Guid ShoppingCartId,
    PricedProductItem ProductItem
);

public record ShoppingCartConfirmed(
    Guid ShoppingCartId,
    DateTime ConfirmedAt
);

public record ShoppingCartCanceled(
    Guid ShoppingCartId,
    DateTime CanceledAt
);

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4,

    Closed = Confirmed | Canceled
}

public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    ProductItemsList ProductItems,
    DateTime? ConfirmedAt = null,
    DateTime? CanceledAt = null
)
{
    public bool IsClosed => ShoppingCartStatus.Closed.HasFlag(Status);

    public static ShoppingCart Evolve(ShoppingCart entity, object @event) =>
        @event switch
        {
            ShoppingCartOpened (var cartId, var clientId) =>
                entity with
                {
                    Id = cartId,
                    ClientId = clientId,
                    Status = ShoppingCartStatus.Pending,
                    ProductItems = ProductItemsList.Empty()
                },

            ProductItemAddedToShoppingCart (_, var productItem) =>
                entity with
                {
                    ProductItems = entity.ProductItems.Add(productItem)
                },

            ProductItemRemovedFromShoppingCart (_, var productItem) =>
                entity with
                {
                    ProductItems = entity.ProductItems.Remove(productItem)
                },

            ShoppingCartConfirmed (_, var confirmedAt) =>
                entity with
                {
                    Status = ShoppingCartStatus.Confirmed,
                    ConfirmedAt = confirmedAt
                },

            ShoppingCartCanceled (_, var canceledAt) =>
                entity with
                {
                    Status = ShoppingCartStatus.Canceled,
                    CanceledAt = canceledAt
                },
            _ => entity
        };

    public static ShoppingCart Default() =>
        new (Guid.Empty, Guid.Empty, default, ProductItemsList.Empty(), null, null);

    public static string MapToStreamId(Guid shoppingCartId) =>
        $"ShoppingCart-{shoppingCartId}";
}
