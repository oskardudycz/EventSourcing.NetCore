namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution3;

using static ShoppingCartEvent;

public abstract record ShoppingCartEvent
{
    public record Opened(ClientId ClientId, DateTimeOffset OpenedAt): ShoppingCartEvent;
    public record ProductItemAdded(PricedProductItem ProductItem): ShoppingCartEvent;
    public record ProductItemRemoved(PricedProductItem ProductItem): ShoppingCartEvent;
    public record Confirmed(DateTimeOffset ConfirmedAt): ShoppingCartEvent;
    public record Canceled(DateTimeOffset CanceledAt): ShoppingCartEvent;
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
    }

    public record Closed: ShoppingCart;

    public static ShoppingCart Create(Opened opened) =>
        new Pending(Array.Empty<(ProductId ProductId, int Quantity)>());

    public ShoppingCart Apply(ProductItemAdded productItemAdded) =>
        this is Pending pending
            ? pending with
            {
                ProductItems = pending.ProductItems
                    .Concat(new[]
                    {
                        (productItemAdded.ProductItem.ProductId, productItemAdded.ProductItem.Quantity.Value)
                    })
                    .ToArray()
            }
            : this;

    public ShoppingCart Apply(ProductItemRemoved productItemRemoved) =>
        this is Pending pending
            ? pending with
            {
                ProductItems = pending.ProductItems
                    .Concat(new[]
                    {
                        (productItemRemoved.ProductItem.ProductId, -productItemRemoved.ProductItem.Quantity.Value)
                    })
                    .ToArray()
            }
            : this;

    public ShoppingCart Apply(Confirmed confirmed) =>
        this is Pending pending ? new Closed() : this;

    public ShoppingCart Apply(Canceled canceled) =>
        this is Pending pending ? new Closed() : this;

    public Guid Id { get; set; } // Marten unfortunately forces you to have Id
    private ShoppingCart() { } // Not to allow inheritance
}
