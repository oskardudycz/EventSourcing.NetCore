using IntroductionToEventSourcing.BusinessLogic.Mutable;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable;
using static ShoppingCartEvent;
using static ShoppingCartCommand;

public abstract record ShoppingCartCommand
{
    public record OpenShoppingCart(
        Guid ShoppingCartId,
        Guid ClientId
    ): ShoppingCartCommand;

    public record AddProductItemToShoppingCart(
        Guid ShoppingCartId,
        ProductItem ProductItem
    );

    public record RemoveProductItemFromShoppingCart(
        Guid ShoppingCartId,
        PricedProductItem ProductItem
    );

    public record ConfirmShoppingCart(
        Guid ShoppingCartId
    );

    public record CancelShoppingCart(
        Guid ShoppingCartId
    ): ShoppingCartCommand;

    private ShoppingCartCommand() {}
}

public static class ShoppingCartService
{
    public static ShoppingCartOpened Handle(OpenShoppingCart command)
    {
        throw new NotImplementedException("Fill the implementation part");
    }

    public static ProductItemAddedToShoppingCart Handle(
        IProductPriceCalculator priceCalculator,
        AddProductItemToShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        throw new NotImplementedException("Fill the implementation part");
    }

    public static ProductItemRemovedFromShoppingCart Handle(
        RemoveProductItemFromShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        throw new NotImplementedException("Fill the implementation part");
    }

    public static ShoppingCartConfirmed Handle(ConfirmShoppingCart command, ShoppingCart shoppingCart)
    {
        throw new NotImplementedException("Fill the implementation part");
    }

    public static ShoppingCartCanceled Handle(CancelShoppingCart command, ShoppingCart shoppingCart)
    {
        throw new NotImplementedException("Fill the implementation part");
    }
}
