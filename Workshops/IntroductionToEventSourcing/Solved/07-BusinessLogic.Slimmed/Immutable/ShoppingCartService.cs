using IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable.Products;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;

using static ShoppingCart.Event;
using static ShoppingCartCommand;
using static ShoppingCart;

public abstract record ShoppingCartCommand
{
    public record OpenShoppingCart(
        Guid ShoppingCartId,
        Guid ClientId,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record AddProductItemToShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record RemoveProductItemFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record ConfirmShoppingCart(
        Guid ShoppingCartId,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record CancelShoppingCart(
        Guid ShoppingCartId,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    private ShoppingCartCommand() { }
}

public static class ShoppingCartService
{
    public static Event Handle(ShoppingCartCommand command, ShoppingCart state) =>
        (state, command) switch
        {
            (Initial, OpenShoppingCart(_, var clientId, var now)) =>
                new Opened(clientId, now),

            (Pending, AddProductItemToShoppingCart(_, var productItem, var now)) =>
                new ProductItemAdded(productItem, now),

            (Pending(var productItems),
                RemoveProductItemFromShoppingCart(_, var productItem, var now)) =>
                productItems.HasEnough(productItem)
                    ? new ProductItemRemoved(productItem, now)
                    : throw new InvalidOperationException("Not enough product items to remove"),

            (Pending, ConfirmShoppingCart(_, var now)) =>
                new Confirmed(now),

            (Pending, CancelShoppingCart(_, var now)) =>
                new Canceled(now),

            _ => throw new InvalidOperationException($"Cannot {command.GetType().Name} for {state.GetType().Name} shopping cart")
        };
}
