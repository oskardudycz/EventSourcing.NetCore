namespace IntroductionToEventSourcing.BusinessLogic.Mixed;
using static ShoppingCartEvent;

public interface IAggregate
{
    public Guid Id { get; }

    public void Evolve(object @event) { }
}


// COMMANDS
public record OpenShoppingCart(
    Guid ShoppingCartId,
    Guid ClientId
)
{
    public static OpenShoppingCart From(Guid? cartId, Guid? clientId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (clientId == null || clientId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(clientId));

        return new OpenShoppingCart(cartId.Value, clientId.Value);
    }
}

public record AddProductItemToShoppingCart(
    Guid ShoppingCartId,
    ProductItem ProductItem
)
{
    public static AddProductItemToShoppingCart From(Guid? cartId, ProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new AddProductItemToShoppingCart(cartId.Value, productItem);
    }
}

public record RemoveProductItemFromShoppingCart(
    Guid ShoppingCartId,
    PricedProductItem ProductItem
)
{
    public static RemoveProductItemFromShoppingCart From(Guid? cartId, PricedProductItem? productItem)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));
        if (productItem == null)
            throw new ArgumentOutOfRangeException(nameof(productItem));

        return new RemoveProductItemFromShoppingCart(cartId.Value, productItem);
    }
}

public record ConfirmShoppingCart(
    Guid ShoppingCartId
)
{
    public static ConfirmShoppingCart From(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new ConfirmShoppingCart(cartId.Value);
    }
}

public record CancelShoppingCart(
    Guid ShoppingCartId
)
{
    public static CancelShoppingCart From(Guid? cartId)
    {
        if (cartId == null || cartId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(cartId));

        return new CancelShoppingCart(cartId.Value);
    }
}

public class ShoppingCart: IAggregate
{
    public Guid Id { get; private set;  }
    public Guid ClientId { get; private set; }
    public ShoppingCartStatus Status { get; private set; }
    public IList<PricedProductItem> ProductItems { get; } = new List<PricedProductItem>();
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CanceledAt { get; private set; }

    public bool IsClosed => ShoppingCartStatus.Closed.HasFlag(Status);

    public void Evolve(object @event)
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

    public static (ShoppingCartOpened Event, ShoppingCart Aggregate) Open(
        Guid cartId,
        Guid clientId)
    {
        var @event = new ShoppingCartOpened(
            cartId,
            clientId
        );



        return (@event, new ShoppingCart(@event));
    }

    private ShoppingCart(ShoppingCartOpened @event) =>
        Apply(@event);

    //just for default creation of empty object
    private ShoppingCart() { }

    private void Apply(ShoppingCartOpened opened)
    {
        Id = opened.ShoppingCartId;
        ClientId = opened.ClientId;
        Status = ShoppingCartStatus.Pending;
    }

    public ProductItemAddedToShoppingCart AddProduct(
        IProductPriceCalculator productPriceCalculator,
        ProductItem productItem)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{Status}' status is not allowed.");

        var pricedProductItem = productPriceCalculator.Calculate(productItem);

        var @event = new ProductItemAddedToShoppingCart(Id, pricedProductItem);

        Apply(@event);

        return @event;
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
            ProductItems[ProductItems.IndexOf(current)] =
                new PricedProductItem(current.ProductId, current.Quantity + quantityToAdd, current.UnitPrice);
    }

    public ProductItemRemovedFromShoppingCart RemoveProduct(PricedProductItem productItemToBeRemoved)
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Removing product item for cart in '{Status}' status is not allowed.");

        if (!HasEnough(productItemToBeRemoved))
            throw new InvalidOperationException("Not enough product items to remove");

        var @event = new ProductItemRemovedFromShoppingCart(Id, productItemToBeRemoved);

        Apply(@event);

        return @event;
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
            ProductItems[ProductItems.IndexOf(current)] =
                new PricedProductItem(current.ProductId, current.Quantity - quantityToRemove, current.UnitPrice);
    }

    public ShoppingCartConfirmed Confirm()
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Confirming cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartConfirmed(Id, DateTime.UtcNow);

        Apply(@event);

        return @event;
    }

    private void Apply(ShoppingCartConfirmed confirmed)
    {
        Status = ShoppingCartStatus.Confirmed;
        ConfirmedAt = confirmed.ConfirmedAt;
    }

    public ShoppingCartCanceled Cancel()
    {
        if (IsClosed)
            throw new InvalidOperationException(
                $"Canceling cart in '{Status}' status is not allowed.");

        var @event = new ShoppingCartCanceled(Id, DateTime.UtcNow);

        Apply(@event);

        return @event;
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
        var (productId, quantity) = productItem;
        return new PricedProductItem(productId, quantity, value);
    }
}
