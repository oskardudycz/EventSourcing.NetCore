namespace ECommerce.Domain.ShoppingCarts.ConfirmingCart;

public record ConfirmShoppingCart(
    Guid CartId
);

public record ShoppingCartConfirmed(
    Guid CartId,
    DateTime ConfirmedAt
);
