using IntroductionToEventSourcing.GettingStateFromEvents.Immutable;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Mutable.Solution1;

public abstract class Aggregate
{
    public Guid Id { get; protected set; } = default!;

    private readonly Queue<object> uncommittedEvents = new();

    public virtual void When(object @event) { }

    public object[] DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToArray();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event)
    {
        uncommittedEvents.Enqueue(@event);
    }
}


public class ShoppingCart: Aggregate
{
    public Guid ClientId { get; private set; }
    public ShoppingCartStatus Status { get; private set; }
    public IList<PricedProductItem> ProductItems { get; } = new List<PricedProductItem>();
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }

    public bool IsClosed => ShoppingCartStatus.Closed.HasFlag(Status);

    public override void When(object @event)
    {
        switch (@event)
        {
            case ShoppingCartOpened opened:
                Apply(opened);
                break;
            case ProductItemAddedToShoppingCart productItemAdded:
                Apply(productItemAdded);
                break;
            case ProductItemRemovedFromShoppingCart productItemRemoved:
                Apply(productItemRemoved);
                break;
            case ShoppingCartConfirmed confirmed:
                Apply(confirmed);
                break;
            case ShoppingCartCanceled canceled:
                Apply(canceled);
                break;
        }
    }

    public static ShoppingCart Open(
        Guid cartId,
        Guid clientId)
    {
        return new ShoppingCart(cartId, clientId);
    }

    private ShoppingCart(
        Guid id,
        Guid clientId)
    {
        var @event = new ShoppingCartOpened(
            id,
            clientId
        );

        Enqueue(@event);
        Apply(@event);
    }

    //just for default creation of empty object
    private ShoppingCart() { }

    private void Apply(ShoppingCartOpened opened)
    {
        Id = opened.ShoppingCartId;
        ClientId = opened.ClientId;
        Status = ShoppingCartStatus.Pending;
    }

    public void AddProduct(
        IProductPriceCalculator productPriceCalculator,
        ProductItem productItem)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem);

        var @event = new ProductItemAddedToShoppingCart(Id, pricedProductItem);

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ProductItemAddedToShoppingCart productItemAdded)
    {
        var (_, pricedProductItem) = productItemAdded;
        var productId = pricedProductItem.ProductId;
        var quantityToAdd = pricedProductItem.Quantity;

        var current = ProductItems.SingleOrDefault(
            pi => pi.ProductId == productId
        );

        if (current == null)
            ProductItems.Add(pricedProductItem);
        else
            current.Quantity += quantityToAdd;
    }

    public void RemoveProduct(PricedProductItem productItemToBeRemoved)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Removing product item for cart in '{Status}' status is not allowed.");

        if (!HasEnough(productItemToBeRemoved))
            throw new InvalidOperationException("Not enough product items to remove");

        var @event = new ProductItemRemovedFromShoppingCart(Id, productItemToBeRemoved);

        Enqueue(@event);
        Apply(@event);
    }

    private bool HasEnough(PricedProductItem productItem)
    {
        var currentQuantity = ProductItems.Where(pi => pi.ProductId == productItem.ProductId)
            .Select(pi => pi.Quantity)
            .FirstOrDefault();

        return currentQuantity >= productItem.Quantity;
    }

    private void Apply(ProductItemRemovedFromShoppingCart productItemRemoved)
    {
        var (_, pricedProductItem) = productItemRemoved;
        var productId = pricedProductItem.ProductId;
        var quantityToRemove = pricedProductItem.Quantity;

        var current = ProductItems.Single(
            pi => pi.ProductId == productId
        );

        if (current.Quantity == quantityToRemove)
            ProductItems.Remove(current);
        else
            current.Quantity -= quantityToRemove;
    }

    public void Confirm()
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Confirming cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartConfirmed(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ShoppingCartConfirmed confirmed)
    {
        Status = ShoppingCartStatus.Confirmed;
        ConfirmedAt = confirmed.ConfirmedAt;
    }

    public void Cancel()
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Canceling cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartCanceled(Id, DateTime.UtcNow);

        Enqueue(@event);
        Apply(@event);
    }

    private void Apply(ShoppingCartCanceled canceled)
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

public interface IProductPriceCalculator
{
    PricedProductItem Calculate(ProductItem productItems);
}

public class FakeProductPriceCalculator: IProductPriceCalculator
{
    private readonly int value;

    private FakeProductPriceCalculator(int value)
    {
        this.value = value;
    }

    public static FakeProductPriceCalculator Returning(int value) => new(value);

    public PricedProductItem Calculate(ProductItem productItem)
    {
        return new PricedProductItem
            {
                ProductId = productItem.ProductId,
                Quantity = productItem.Quantity,
                UnitPrice = value
            };
    }
}
