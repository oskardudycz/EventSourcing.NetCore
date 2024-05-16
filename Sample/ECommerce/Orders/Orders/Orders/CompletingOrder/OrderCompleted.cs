namespace Orders.Orders.CompletingOrder;

public record OrderCompleted(
    Guid OrderId,
    DateTimeOffset CompletedAt
)
{
    public static OrderCompleted Create(Guid orderId, DateTimeOffset completedAt)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (completedAt == default)
            throw new ArgumentOutOfRangeException(nameof(completedAt));

        return new OrderCompleted(orderId, completedAt);
    }
}
