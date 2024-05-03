using IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable.Pricing;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;

using static ShoppingCart.Event;
using static ShoppingCart.Command;

public partial record ShoppingCart
{
    public abstract record Command
    {
        public record Open(
            Guid ClientId,
            DateTimeOffset Now
        ): Command;

        public record AddProductItem(
            PricedProductItem ProductItem,
            DateTimeOffset Now
        ): Command;

        public record RemoveProductItem(
            PricedProductItem ProductItem,
            DateTimeOffset Now
        ): Command;

        public record Confirm(
            DateTimeOffset Now
        ): Command;

        public record Cancel(
            DateTimeOffset Now
        ): Command;

        private Command() { }
    }

    public static Event Decide(Command command, ShoppingCart state) =>
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

            _ => throw new InvalidOperationException(
                $"Cannot {command.GetType().Name} for {state.GetType().Name} shopping cart")
        };
}
