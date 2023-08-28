using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution4;

using static ShoppingCart;
using static ShoppingCartEvent;
using static ShoppingCartCommand;

public abstract record ShoppingCartCommand
{
    public record Open(ShoppingCartId ShoppingCartId, ClientId ClientId, DateTimeOffset Now): ShoppingCartCommand;

    public record AddProductItem(ShoppingCartId ShoppingCartId, PricedProductItem ProductItem): ShoppingCartCommand;

    public record RemoveProductItem(ShoppingCartId ShoppingCartId, PricedProductItem ProductItem): ShoppingCartCommand;

    public record Confirm(ShoppingCartId ShoppingCartId, DateTimeOffset Now): ShoppingCartCommand;

    public record Cancel(ShoppingCartId ShoppingCartId, DateTimeOffset Now): ShoppingCartCommand;
}

// Value Objects
public static class ShoppingCartService
{
    public static ShoppingCartEvent Decide(
        ShoppingCartCommand command,
        ShoppingCart state
    ) =>
        command switch
        {
            Open open => Handle(open),
            AddProductItem addProduct => Handle(addProduct, state.EnsureIsPending()),
            RemoveProductItem removeProduct => Handle(removeProduct, state.EnsureIsPending()),
            Confirm confirm => Handle(confirm, state.EnsureIsPending()),
            Cancel cancel => Handle(cancel, state.EnsureIsPending()),
            _ => throw new InvalidOperationException($"Cannot handle {command.GetType().Name} command")
        };

    private static Opened Handle(Open command) =>
        new Opened(command.ClientId, command.Now);

    private static ProductItemAdded Handle(AddProductItem command, Pending shoppingCart) =>
        new ProductItemAdded(command.ProductItem);

    private static ProductItemRemoved Handle(RemoveProductItem command, Pending shoppingCart) =>
        shoppingCart.HasEnough(command.ProductItem)
            ? new ProductItemRemoved(command.ProductItem)
            : throw new InvalidOperationException("Not enough product items to remove.");

    private static Confirmed Handle(Confirm command, Pending shoppingCart) =>
        shoppingCart.HasItems
            ? new Confirmed(DateTime.UtcNow)
            : throw new InvalidOperationException("Shopping cart is empty!");

    private static Canceled Handle(Cancel command, Pending shoppingCart) =>
        new Canceled(DateTime.UtcNow);

    private static Pending EnsureIsPending(this ShoppingCart shoppingCart) =>
        shoppingCart as Pending ?? throw new InvalidOperationException(
            $"Invalid operation for '{shoppingCart.GetType().Name}' shopping card.");
}

public static class ShoppingCartDocumentSessionExtensions
{
    public static Task Decide(
        this IDocumentSession session,
        ShoppingCartId streamId,
        ShoppingCartCommand command,
        CancellationToken ct = default
    ) =>
        session.Decide<ShoppingCart, ShoppingCartCommand, ShoppingCartEvent>(
            (c, s) => new[] { ShoppingCartService.Decide(c, s) },
            () => new Empty(),
            streamId.Value,
            command,
            ct
        );
}
