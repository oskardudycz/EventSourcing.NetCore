using IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable.Products;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;

using static ShoppingCart.Event;

// EVENTS

public abstract record ShoppingCart
{
    public abstract record Event
    {
        public record Opened(
            Guid ClientId,
            DateTimeOffset OpenedAt
        ): Event;

        public record ProductItemAdded(
            PricedProductItem ProductItem,
            DateTimeOffset AddedAt
        ): Event;

        public record ProductItemRemoved(
            PricedProductItem ProductItem,
            DateTimeOffset RemovedAt
        ): Event;

        public record Confirmed(
            DateTimeOffset ConfirmedAt
        ): Event;

        public record Canceled(
            DateTimeOffset CanceledAt
        ): Event;

        // This won't allow external inheritance
        private Event() { }
    }

    public record Initial: ShoppingCart;

    public record Pending(ProductItems ProductItems): ShoppingCart;

    public record Closed: ShoppingCart;

    public static ShoppingCart Evolve(ShoppingCart state, Event @event) =>
        (state, @event) switch
        {
            (Initial, Opened) =>
                new Pending(ProductItems.Empty),

            (Pending(var productItems), ProductItemAdded(var productItem, _)) =>
                new Pending(productItems.Add(productItem)),

            (Pending(var productItems), ProductItemRemoved(var productItem, _)) =>
                new Pending(productItems.Remove(productItem)),

            (Pending, Confirmed) =>
                new Closed(),

            (Pending, Canceled) =>
                new Closed(),

            _ => state
        };
}

