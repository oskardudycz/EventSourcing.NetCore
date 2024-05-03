namespace IntroductionToEventSourcing.BusinessLogic.Slimmed.Immutable;
using static ShoppingCartEvent;
using static ShoppingCartCommand;

public abstract record ShoppingCartCommand
{
    public record OpenShoppingCart(
        Guid ShoppingCartId,
        Guid ClientId,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    public record AddProductItemToShoppingCart(
        Guid ShoppingCartId,
        ProductItem ProductItem,
        DateTimeOffset Now
    );

    public record RemoveProductItemFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem,
        DateTimeOffset Now
    );

    public record ConfirmShoppingCart(
        Guid ShoppingCartId,
        DateTimeOffset Now
    );

    public record CancelShoppingCart(
        Guid ShoppingCartId,
        DateTimeOffset Now
    ): ShoppingCartCommand;

    private ShoppingCartCommand() {}
}

public static class ShoppingCartService
{
    public static ShoppingCartOpened Handle(OpenShoppingCart command)
    {
        var (shoppingCartId, clientId, now) = command;

        return new ShoppingCartOpened(
            shoppingCartId,
            clientId,
            now
        );
    }

    public static ProductItemAddedToShoppingCart Handle(
        IProductPriceCalculator priceCalculator,
        AddProductItemToShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        var (cartId, productItem, now) = command;

        if (shoppingCart.IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        var pricedProductItem = priceCalculator.Calculate(productItem);

        return new ProductItemAddedToShoppingCart(
            cartId,
            pricedProductItem,
            now
        );
    }

    public static ProductItemRemovedFromShoppingCart Handle(
        RemoveProductItemFromShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        var (cartId, productItem, now) = command;

        if (shoppingCart.IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        if (!shoppingCart.HasEnough(productItem))
            throw new InvalidOperationException("Not enough product items to remove");

        return new ProductItemRemovedFromShoppingCart(
            cartId,
            productItem,
            now
        );
    }

    public static ShoppingCartConfirmed Handle(ConfirmShoppingCart command, ShoppingCart shoppingCart)
    {
        if (shoppingCart.IsClosed)
            throw new InvalidOperationException($"Confirming cart in '{shoppingCart.Status}' status is not allowed.");

        if(shoppingCart.ProductItems.Length == 0)
            throw new InvalidOperationException($"Cannot confirm empty shopping cart");

        return new ShoppingCartConfirmed(
            shoppingCart.Id,
            command.Now
        );
    }

    public static ShoppingCartCanceled Handle(CancelShoppingCart command, ShoppingCart shoppingCart)
    {
        if (shoppingCart.IsClosed)
            throw new InvalidOperationException($"Canceling cart in '{shoppingCart.Status}' status is not allowed.");

        return new ShoppingCartCanceled(
            shoppingCart.Id,
            command.Now
        );
    }
}
