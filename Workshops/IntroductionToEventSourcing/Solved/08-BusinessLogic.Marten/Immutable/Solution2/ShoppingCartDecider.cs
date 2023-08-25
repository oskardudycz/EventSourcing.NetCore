using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution2;
using static ShoppingCartEvent;
using static ShoppingCartCommand;

public abstract record ShoppingCartCommand
{
    public record OpenShoppingCart(
        Guid ShoppingCartId,
        Guid ClientId
    ): ShoppingCartCommand
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
        PricedProductItem ProductItem
    ): ShoppingCartCommand
    {
        public static AddProductItemToShoppingCart From(Guid? cartId, PricedProductItem? productItem)
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
    ): ShoppingCartCommand
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
    ): ShoppingCartCommand
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
    ): ShoppingCartCommand
    {
        public static CancelShoppingCart From(Guid? cartId)
        {
            if (cartId == null || cartId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(cartId));

            return new CancelShoppingCart(cartId.Value);
        }
    }
}

public static class ShoppingCartService
{
    public static ShoppingCartEvent Decide(
        ShoppingCartCommand command,
        ShoppingCart state
    ) =>
        command switch
        {
            OpenShoppingCart open  => Handle(open),
            AddProductItemToShoppingCart addProduct => Handle(addProduct, state),
            RemoveProductItemFromShoppingCart removeProduct => Handle(removeProduct, state),
            ConfirmShoppingCart confirm => Handle(confirm, state),
            CancelShoppingCart cancel => Handle(cancel, state),
            _ => throw new InvalidOperationException($"Cannot handle {command.GetType().Name} command")
        };

    private static ShoppingCartOpened Handle(OpenShoppingCart command)
    {
        var (shoppingCartId, clientId) = command;

        return new ShoppingCartOpened(
            shoppingCartId,
            clientId
        );
    }

    private static ProductItemAddedToShoppingCart Handle(
        AddProductItemToShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        var (cartId, pricedProductItem) = command;

        if (shoppingCart.IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        return new ProductItemAddedToShoppingCart(
            cartId,
            pricedProductItem
        );
    }

    private static ProductItemRemovedFromShoppingCart Handle(
        RemoveProductItemFromShoppingCart command,
        ShoppingCart shoppingCart
    )
    {
        var (cartId, productItem) = command;

        if (shoppingCart.IsClosed)
            throw new InvalidOperationException(
                $"Adding product item for cart in '{shoppingCart.Status}' status is not allowed.");

        if (!shoppingCart.HasEnough(productItem))
            throw new InvalidOperationException("Not enough product items to remove");

        return new ProductItemRemovedFromShoppingCart(
            cartId,
            productItem
        );
    }

    private static ShoppingCartConfirmed Handle(ConfirmShoppingCart command, ShoppingCart shoppingCart)
    {
        if (shoppingCart.IsClosed)
            throw new InvalidOperationException($"Confirming cart in '{shoppingCart.Status}' status is not allowed.");

        return new ShoppingCartConfirmed(
            shoppingCart.Id,
            DateTime.UtcNow
        );
    }

    private static ShoppingCartCanceled Handle(CancelShoppingCart command, ShoppingCart shoppingCart)
    {
        if (shoppingCart.IsClosed)
            throw new InvalidOperationException($"Canceling cart in '{shoppingCart.Status}' status is not allowed.");

        return new ShoppingCartCanceled(
            shoppingCart.Id,
            DateTime.UtcNow
        );
    }
}

public static class ShoppingCartDocumentCommandHandler
{
    public static Task Decide(
        this IDocumentSession session,
        Guid streamId,
        ShoppingCartCommand command,
        CancellationToken ct = default
    ) =>
        session.Decide<ShoppingCart, ShoppingCartCommand, ShoppingCartEvent>(
            (c, s) => new[] { ShoppingCartService.Decide(c, s) },
            streamId,
            command,
            ct
        );
}
