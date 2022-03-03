using ECommerce.ShoppingCarts.ProductItems;

namespace ECommerce.ShoppingCarts;

public record ShoppingCartInitialized(
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

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Cancelled = 4,

    Closed = Confirmed | Cancelled
}

public record ShoppingCart(
    Guid Id,
    Guid ClientId,
    ShoppingCartStatus Status,
    ProductItemsList ProductItems,
    DateTime? ConfirmedAt = null
)
{
    public bool IsClosed { get; } = Status.HasFlag(ShoppingCartStatus.Closed);

    public static ShoppingCart When(ShoppingCart entity, object @event)
    {
        return @event switch
        {
            ShoppingCartInitialized (var cartId, var clientId) =>
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
            _ => entity
        };
    }

    public static ShoppingCart Default() =>
        new (default, default, default, ProductItemsList.Empty(), default);

    public static string MapToStreamId(Guid shoppingCartId) =>
        $"ShoppingCart-{shoppingCartId}";
}