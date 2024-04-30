namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution4;

using static ShoppingCartEvent;

public abstract record ShoppingCartEvent
{
    public record Opened(ClientId ClientId, DateTimeOffset OpenedAt): ShoppingCartEvent;

    public record ProductItemAdded(PricedProductItem ProductItem): ShoppingCartEvent;

    public record ProductItemRemoved(PricedProductItem ProductItem): ShoppingCartEvent;

    public record Confirmed(DateTimeOffset ConfirmedAt): ShoppingCartEvent;

    public record Canceled(DateTimeOffset CanceledAt): ShoppingCartEvent;

    // This won't allow external inheritance
    private ShoppingCartEvent(){}
}

public record ShoppingCart
{
    public record Empty: ShoppingCart;

    public record Pending((ProductId ProductId, int Quantity)[] ProductItems): ShoppingCart
    {
        public bool HasEnough(PricedProductItem productItem) =>
            ProductItems
                .Where(pi => pi.ProductId == productItem.ProductId)
                .Sum(pi => pi.Quantity) >= productItem.Quantity.Value;

        public bool HasItems { get; } =
            ProductItems.Sum(pi => pi.Quantity) <= 0;
    }

    public record Closed: ShoppingCart;

    public ShoppingCart Apply(ShoppingCartEvent @event) =>
        @event switch
        {
            Opened =>
                new Pending([]),

            ProductItemAdded (var (productId, quantity, _)) =>
                this is Pending pending
                    ? pending with
                    {
                        ProductItems = pending.ProductItems
                            .Concat(new[] { (productId, quantity.Value) })
                            .ToArray()
                    }
                    : this,

            ProductItemRemoved (var (productId, quantity, _)) =>
                this is Pending pending
                    ? pending with
                    {
                        ProductItems = pending.ProductItems
                            .Concat(new[] { (productId, -quantity.Value) })
                            .ToArray()
                    }
                    : this,

            Confirmed =>
                this is Pending ? new Closed() : this,

            Canceled =>
                this is Pending ? new Closed() : this,

            _ => this
        };

    public Guid Id { get; set; } // Marten unfortunately forces you to have Id
    private ShoppingCart() { } // Not to allow inheritance
}
