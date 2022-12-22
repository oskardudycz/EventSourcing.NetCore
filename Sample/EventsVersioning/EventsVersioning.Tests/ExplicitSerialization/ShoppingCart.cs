using System.Text.Json;
using System.Text.Json.Nodes;

namespace EventsVersioning.Tests.ExplicitSerialization;

using static ShoppingCartEvent;

public abstract class StronglyTypedValue<T>: IEquatable<StronglyTypedValue<T>> where T : notnull
{
    public T Value { get; }

    protected StronglyTypedValue(T value) => Value = value;

    public override string ToString() => Value.ToString()!;

    public bool Equals(StronglyTypedValue<T>? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((StronglyTypedValue<T>)obj);
    }

    public override int GetHashCode() =>
        EqualityComparer<T>.Default.GetHashCode(Value);

    public static bool operator ==(StronglyTypedValue<T>? left, StronglyTypedValue<T>? right) =>
        Equals(left, right);

    public static bool operator !=(StronglyTypedValue<T>? left, StronglyTypedValue<T>? right) =>
        !Equals(left, right);
}

public class ClientId: StronglyTypedValue<Guid>
{
    private ClientId(Guid value): base(value) { }

    public static readonly ClientId Unknown = new(Guid.Empty);

    public static ClientId New() => new(Guid.NewGuid());

    public static ClientId Parse(string? value)
    {
        if (!Guid.TryParse(value, out var guidValue) || guidValue == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(value));

        return new ClientId(guidValue);
    }
}

public class ProductId: StronglyTypedValue<Guid>
{
    private ProductId(Guid value): base(value) { }

    public static readonly ProductId Unknown = new(Guid.Empty);

    public static ProductId New() => new(Guid.NewGuid());

    public static ProductId Parse(string? value)
    {
        if (!Guid.TryParse(value, out var guidValue) || guidValue == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(value));

        return new ProductId(guidValue);
    }
}

public class ShoppingCartId: StronglyTypedValue<Guid>
{
    private ShoppingCartId(Guid value): base(value)
    {
    }

    public static readonly ShoppingCartId Unknown = new(Guid.Empty);

    public static ShoppingCartId New() => new(Guid.NewGuid());

    public static ShoppingCartId Parse(string? value)
    {
        if (!Guid.TryParse(value, out var guidValue) || guidValue == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(value));

        return new ShoppingCartId(guidValue);
    }
}

public enum Currency
{
    USD,
    EUR,
    PLN
}

public class Amount: StronglyTypedValue<int>, IComparable<Amount>
{
    private Amount(int value): base(value) { }
    public bool IsPositive => Value > 0;

    public int CompareTo(Amount? other) => Value.CompareTo(other?.Value);

    public static Amount Parse(int value) => new(value);
}

public class Quantity: StronglyTypedValue<uint>, IComparable<Quantity>, IComparable<int>
{
    private Quantity(uint value): base(value) { }

    public int CompareTo(Quantity? other) => Value.CompareTo(other?.Value);
    public int CompareTo(int other) => Value.CompareTo(other);

    public static Quantity operator +(Quantity a) => a;

    public static Quantity operator -(Quantity _) => throw new InvalidOperationException();

    public static Quantity operator +(Quantity a, Quantity b) =>
        new(a.Value + b.Value);

    public static Quantity operator -(Quantity a, Quantity b) =>
        new(a.Value - b.Value);

    public static bool operator >(Quantity a, Quantity b)
        => a.Value > b.Value;

    public static bool operator >=(Quantity a, Quantity b)
        => a.Value >= b.Value;

    public static bool operator <(Quantity a, Quantity b)
        => a.Value < b.Value;

    public static bool operator <=(Quantity a, Quantity b)
        => a.Value <= b.Value;

    public static Quantity Parse(uint value) => new(value);
}

public class LocalDateTime: StronglyTypedValue<DateTimeOffset>, IComparable<LocalDateTime>
{
    private LocalDateTime(DateTimeOffset value): base(value)
    {
    }

    public int CompareTo(LocalDateTime? other) => other != null ? Value.CompareTo(other.Value) : -1;


    public static LocalDateTime Parse(DateTimeOffset value) => new(value);
}

public record Money(
    Amount Amount,
    Currency Currency
);

public class Price: StronglyTypedValue<Money>
{
    public Price(Money value): base(value) { }

    public static Price Parse(Money value)
    {
        if (!value.Amount.IsPositive)
            throw new ArgumentOutOfRangeException(nameof(value), "Price cannot be negative");

        return new Price(value);
    }
}

public abstract record ShoppingCartEvent
{
    public record ShoppingCartOpened(
        ShoppingCartId ShoppingCartId,
        ClientId ClientId
    ): ShoppingCartEvent;

    public record ProductItemAddedToShoppingCart(
        ShoppingCartId ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ProductItemRemovedFromShoppingCart(
        ShoppingCartId ShoppingCartId,
        PricedProductItem ProductItem
    ): ShoppingCartEvent;

    public record ShoppingCartConfirmed(
        ShoppingCartId ShoppingCartId,
        LocalDateTime ConfirmedAt
    ): ShoppingCartEvent;

    public record ShoppingCartCanceled(
        ShoppingCartId ShoppingCartId,
        LocalDateTime CanceledAt
    ): ShoppingCartEvent;
}

public enum ShoppingCartStatus
{
    Pending = 1,
    Confirmed = 2,
    Canceled = 4,

    Closed = Confirmed | Canceled
}

public record PricedProductItem(
    ProductId ProductId,
    Quantity Quantity,
    Price UnitPrice
);

public record ShoppingCart(
    ShoppingCartId Id,
    ClientId ClientId,
    ShoppingCartStatus Status,
    Dictionary<ProductId, Quantity> ProductItems
)
{
    public static ShoppingCart When(ShoppingCart entity, object @event)
    {
        return @event switch
        {
            ShoppingCartOpened (var cartId, var clientId) =>
                entity with
                {
                    Id = cartId,
                    ClientId = clientId,
                    Status = ShoppingCartStatus.Pending,
                    ProductItems = new Dictionary<ProductId, Quantity>()
                },

            ProductItemAddedToShoppingCart (_, var productItem) =>
                entity with { ProductItems = entity.ProductItems.Add(productItem) },

            ProductItemRemovedFromShoppingCart (_, var productItem) =>
                entity with { ProductItems = entity.ProductItems.Remove(productItem) },

            ShoppingCartConfirmed (_, var confirmedAt) =>
                entity with { Status = ShoppingCartStatus.Confirmed },

            ShoppingCartCanceled (_, var canceledAt) =>
                entity with { Status = ShoppingCartStatus.Canceled },
            _ => entity
        };
    }

    public static ShoppingCart Default =>
        new(ShoppingCartId.Unknown, ClientId.Unknown, default, new Dictionary<ProductId, Quantity>());
}

public static class ProductItemsExtensions
{
    public static Dictionary<ProductId, Quantity> Add(this Dictionary<ProductId, Quantity> productItems,
        PricedProductItem productItem) =>
        productItems
            .Union(new[] { new KeyValuePair<ProductId, Quantity>(productItem.ProductId, productItem.Quantity) })
            .GroupBy(ks => ks.Key)
            .ToDictionary(ks => ks.Key, ps => Quantity.Parse((uint)ps.Sum(x => x.Value.Value)));

    public static Dictionary<ProductId, Quantity>
        Remove(this Dictionary<ProductId, Quantity> productItems, PricedProductItem productItem) =>
        productItems
            .Select(p =>
                p.Key == productItem.ProductId
                    ? new KeyValuePair<ProductId, Quantity>(p.Key,
                        Quantity.Parse(p.Value.Value - productItem.Quantity.Value))
                    : p)
            .Where(p => p.Value > Quantity.Parse(0))
            .ToDictionary(ks => ks.Key, ps => ps.Value);
}

public class ShoppingCartEventsSerde
{
    public (string EventType, JsonObject Data) Serialize(ShoppingCartEvent @event)
    {
        return @event switch
        {
            ShoppingCartOpened e =>
                ("shopping_cart_opened",
                    Json.Object(
                        Json.Node("shoppingCartId", e.ShoppingCartId.ToJson()),
                        Json.Node("clientId", e.ClientId.ToJson()
                        )
                    )
                ),
            ProductItemAddedToShoppingCart e =>
                ("product_item_added_to_shopping_cart",
                    Json.Object(
                        Json.Node("shoppingCartId", e.ShoppingCartId.ToJson()),
                        Json.Node("productItem", e.ProductItem.ToJson())
                    )
                ),
            ProductItemRemovedFromShoppingCart e =>
                ("product_item_removed_from_shopping_cart",
                    Json.Object(
                        Json.Node("shoppingCartId", e.ShoppingCartId.ToJson()),
                        Json.Node("productItem", e.ProductItem.ToJson())
                    )
                ),
            ShoppingCartConfirmed e =>
                ("shopping_cart_confirmed",
                    Json.Object(
                        Json.Node("shoppingCartId", e.ShoppingCartId.ToJson()),
                        Json.Node("confirmedAt", e.ConfirmedAt.ToJson())
                    )
                ),
            ShoppingCartCanceled e =>
                ("shopping_cart_canceled",
                    Json.Object(
                        Json.Node("shoppingCartId", e.ShoppingCartId.ToJson()),
                        Json.Node("canceledAt", e.CanceledAt.ToJson())
                    )
                ),
            _ => throw new InvalidOperationException()
        };
    }

    public ShoppingCartEvent Deserialize(string eventType, JsonDocument document)
    {
        var data = document.RootElement;

        return eventType switch
        {
            "shopping_cart_opened" =>
                new ShoppingCartOpened(
                    data.GetProperty("shoppingCartId").ToShoppingCartId(),
                    data.GetProperty("clientId").ToClientId()
                ),
            "product_item_added_to_shopping_cart" =>
                new ProductItemAddedToShoppingCart(
                    data.GetProperty("shoppingCartId").ToShoppingCartId(),
                    data.GetProperty("productItem").ToPricedProductItem()
                ),
            "product_item_removed_from_shopping_cart" =>
                new ProductItemRemovedFromShoppingCart(
                    data.GetProperty("shoppingCartId").ToShoppingCartId(),
                    data.GetProperty("productItem").ToPricedProductItem()
                ),
            "shopping_cart_confirmed" =>
                new ShoppingCartConfirmed(
                    data.GetProperty("shoppingCartId").ToShoppingCartId(),
                    data.GetProperty("confirmedAt").ToLocalDateTime()
                ),
            "shopping_cart_canceled" =>
                new ShoppingCartCanceled(
                    data.GetProperty("shoppingCartId").ToShoppingCartId(),
                    data.GetProperty("canceledAt").ToLocalDateTime()
                ),
            _ => throw new InvalidOperationException()
        };
    }
}

public static class Json
{
    public static JsonObject Object(params KeyValuePair<string, JsonNode?>[] nodes) => new(nodes);
    public static KeyValuePair<string, JsonNode?> Node(string key, JsonNode? node) => new(key, node);

    public static JsonNode ToJson(this ShoppingCartId value) => value.Value;
    public static JsonNode ToJson(this ProductId value) => value.Value;
    public static JsonNode ToJson(this ClientId value) => value.Value;
    public static JsonNode ToJson(this Amount value) => value.Value;
    public static JsonNode ToJson(this Quantity value) => value.Value;
    public static JsonNode ToJson(this LocalDateTime value) => value.Value;

    public static JsonObject ToJson(this Money value) =>
        Object(
            Node("amount", value.Amount.ToJson()),
            Node("currency", value.Currency.ToString())
        );

    public static JsonObject ToJson(this Price value) => value.Value.ToJson();

    public static JsonObject ToJson(this PricedProductItem value) =>
        Object(
            Node("productId", value.ProductId.ToJson()),
            Node("quantity", value.Quantity.ToJson()),
            Node("unitPrice", value.UnitPrice.ToJson())
        );

    public static ShoppingCartId ToShoppingCartId(this JsonElement value) =>
        ShoppingCartId.Parse(value.GetString());

    public static ProductId ToProductId(this JsonElement value) =>
        ProductId.Parse(value.GetString());

    public static ClientId ToClientId(this JsonElement value) =>
        ClientId.Parse(value.GetString());

    public static Currency ToCurrency(this JsonElement value) =>
        Enum.Parse<Currency>(value.GetString() ?? throw new ArgumentOutOfRangeException());

    public static Amount ToAmount(this JsonElement value) =>
        Amount.Parse(value.GetInt32());

    public static Quantity ToQuantity(this JsonElement value) =>
        Quantity.Parse(value.GetUInt32());

    public static Money ToMoney(this JsonElement value) =>
        new(
            value.GetProperty("amount").ToAmount(),
            value.GetProperty("currency").ToCurrency()
        );

    public static LocalDateTime ToLocalDateTime(this JsonElement value) =>
        LocalDateTime.Parse(DateTimeOffset.Parse(value.GetString() ?? throw new ArgumentOutOfRangeException()));

    public static Price ToPrice(this JsonElement value) => new(value.ToMoney());

    public static PricedProductItem ToPricedProductItem(this JsonElement value) =>
        new(
            value.GetProperty("productId").ToProductId(),
            value.GetProperty("quantity").ToQuantity(),
            value.GetProperty("unitPrice").ToPrice()
        );
}
