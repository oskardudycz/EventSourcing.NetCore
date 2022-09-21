namespace SlimmedAggregate;

public record ShoppingCartOpened(
    Guid CartId,
    Guid ClientId
);

public record ProductItemAdded(
    Guid CartId,
    ProductItem ProductItem
);

public record ProductItemRemoved(
    Guid CartId,
    ProductItem ProductItem
);

public record ShoppingCartConfirmed(
    Guid CartId,
    DateTimeOffset ConfirmedAt
);

public record ShoppingCartCanceled(
    Guid CartId,
    DateTimeOffset CanceledAt
);
