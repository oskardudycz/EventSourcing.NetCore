using IntroductionToEventSourcing.BusinessLogic.Slimmed.Mutable.Pricing;

namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Mutable;

using static ShoppingCartEvent;

// EVENTS
public abstract record ShoppingCartEvent
{
    public record Opened(
        Guid ClientId,
        DateTimeOffset OpenedAt
    ): ShoppingCartEvent;

    public record ProductItemAdded(
        PricedProductItem ProductItem,
        DateTimeOffset AddedAt
    ): ShoppingCartEvent;

    public record ProductItemRemoved(
        PricedProductItem ProductItem,
        DateTimeOffset RemovedAt
    ): ShoppingCartEvent;

    public record Confirmed(
        DateTimeOffset ConfirmedAt
    ): ShoppingCartEvent;

    public record Canceled(
        DateTimeOffset CanceledAt
    ): ShoppingCartEvent;

    // This won't allow external inheritance
    private ShoppingCartEvent() { }
}

// VALUE OBJECTS
public class PricedProductItem
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}

public class ProductItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

// ENTITY
public class ShoppingCart: Aggregate<ShoppingCartEvent>
{
    public Guid ClientId { get; private set; }
    public ShoppingCartStatus Status { get; private set; }
    public IList<PricedProductItem> ProductItems { get; } = new List<PricedProductItem>();
    public DateTimeOffset OpenedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? CanceledAt { get; private set; }

    public bool IsClosed => ShoppingCartStatus.Closed.HasFlag(Status);

    public override void Evolve(ShoppingCartEvent @event)
    {
        switch (@event)
        {
            case Opened opened:
                Apply(opened);
                break;
            case ProductItemAdded productItemAdded:
                Apply(productItemAdded);
                break;
            case ProductItemRemoved productItemRemoved:
                Apply(productItemRemoved);
                break;
            case Confirmed confirmed:
                Apply(confirmed);
                break;
            case Canceled canceled:
                Apply(canceled);
                break;
        }
    }

    public static ShoppingCart Open(Guid clientId, DateTimeOffset now) =>
        new(clientId, now);

    public static ShoppingCart Initial() => new();

    private ShoppingCart(Guid clientId, DateTimeOffset now)
    {
        var @event = new Opened(clientId, now);

        Enqueue(@event);
    }

    //just for default creation of empty object
    private ShoppingCart() { }

    private void Apply(Opened opened)
    {
        ClientId = opened.ClientId;
        Status = ShoppingCartStatus.Pending;
    }

    public void AddProduct(
        IProductPriceCalculator productPriceCalculator,
        ProductItem productItem,
        DateTimeOffset now
    )
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem);

        var @event = new ProductItemAdded(pricedProductItem, now);

        Enqueue(@event);
    }

    private void Apply(ProductItemAdded productItemAdded)
    {
        var pricedProductItem = productItemAdded.ProductItem;
        var productId = pricedProductItem.ProductId;
        var quantityToAdd = pricedProductItem.Quantity;

        var current = ProductItems.SingleOrDefault(
            pi => pi.ProductId == productId
        );

        if (current == null)
            ProductItems.Add(pricedProductItem);
        else
            ProductItems[ProductItems.IndexOf(current)] = new PricedProductItem
            {
                Quantity = current.Quantity + quantityToAdd, ProductId = productId, UnitPrice = current.UnitPrice
            };
    }

    public void RemoveProduct(PricedProductItem productItemToBeRemoved, DateTimeOffset now)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Removing product item for cart in '{Status}' status is not allowed.");

        if (!HasEnough(productItemToBeRemoved))
            throw new InvalidOperationException("Not enough product items to remove");

        var @event = new ProductItemRemoved(productItemToBeRemoved, now);

        Enqueue(@event);
    }

    private bool HasEnough(PricedProductItem productItem)
    {
        var currentQuantity = ProductItems.Where(pi => pi.ProductId == productItem.ProductId)
            .Select(pi => pi.Quantity)
            .FirstOrDefault();

        return currentQuantity >= productItem.Quantity;
    }

    private void Apply(ProductItemRemoved productItemRemoved)
    {
        var pricedProductItem = productItemRemoved.ProductItem;
        var productId = pricedProductItem.ProductId;
        var quantityToRemove = pricedProductItem.Quantity;

        var current = ProductItems.Single(
            pi => pi.ProductId == productId
        );

        if (current.Quantity == quantityToRemove)
            ProductItems.Remove(current);
        else ProductItems[ProductItems.IndexOf(current)] = new PricedProductItem
        {
            Quantity = current.Quantity - quantityToRemove, ProductId = productId, UnitPrice = current.UnitPrice
        };
    }

    public void Confirm(DateTimeOffset now)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Confirming cart in '{Status}' status is not allowed.");

        if (ProductItems.Count == 0)
            throw new InvalidOperationException($"Cannot confirm empty shopping cart");

        var @event = new Confirmed(now);

        Enqueue(@event);
    }

    private void Apply(Confirmed confirmed)
    {
        Status = ShoppingCartStatus.Confirmed;
        ConfirmedAt = confirmed.ConfirmedAt;
    }

    public void Cancel(DateTimeOffset now)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Canceling cart in '{Status}' status is not allowed.");

        var @event = new Canceled(now);

        Enqueue(@event);
    }

    private void Apply(Canceled canceled)
    {
        Status = ShoppingCartStatus.Canceled;
        CanceledAt = canceled.CanceledAt;
    }
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4,

    Closed = Confirmed | Canceled
}
