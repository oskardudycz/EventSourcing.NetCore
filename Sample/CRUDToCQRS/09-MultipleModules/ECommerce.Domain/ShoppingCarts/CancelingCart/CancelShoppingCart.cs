namespace ECommerce.Domain.ShoppingCarts.CancelingCart;

public record CancelShoppingCart(
    Guid CartId
);

public record ShoppingCartCanceled(
    Guid CartId,
    DateTime CanceledAt
);
