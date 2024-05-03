using IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable.Pricing;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;

using static ShoppingCart.Event;
using static ShoppingCartCommand;
using static ShoppingCart;

public abstract record ShoppingCartCommand
{
    public record Open(
        Guid ClientId,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record AddProductItem(
        PricedProductItem ProductItem,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record RemoveProductItem(
        PricedProductItem ProductItem,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record Confirm(
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record Cancel(
        DateTimeOffset Now
    ): ShoppingCartCommand;

    private ShoppingCartCommand() { }
}

public static class ShoppingCartService
{
    public static Event Decide(ShoppingCartCommand command, ShoppingCart state) =>
        (state, command) switch
        {
            (Initial, Open(var clientId, var now)) =>
                new Opened(clientId, now),

            (Pending, AddProductItem(var productItem, var now)) =>
                new ProductItemAdded(productItem, now),

            (Pending(var productItems), RemoveProductItem(var productItem, var now)) =>
                productItems.HasEnough(productItem)
                    ? new ProductItemRemoved(productItem, now)
                    : throw new InvalidOperationException("Not enough product items to remove"),

            (Pending, Confirm(var now)) =>
                new Confirmed(now),

            (Pending, Cancel(var now)) =>
                new Canceled(now),

            _ => throw new InvalidOperationException($"Cannot {command.GetType().Name} for {state.GetType().Name} shopping cart")
        };
}
