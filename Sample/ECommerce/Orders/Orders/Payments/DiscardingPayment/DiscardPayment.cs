using Core.Commands;

namespace Orders.Payments.DiscardingPayment;

public record DiscardPayment(
    Guid PaymentId,
    DiscardReason DiscardReason
)
{
    public static DiscardPayment Create(Guid paymentId)
    {
        if (paymentId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(paymentId));

        return new DiscardPayment(paymentId, DiscardReason.OrderCancelled);
    }
}

public class HandleDiscardPayment(PaymentsApiClient client): ICommandHandler<DiscardPayment>
{
    public async Task Handle(DiscardPayment command, CancellationToken ct)
    {
        var result = await client.Discard(command, ct);

        result.EnsureSuccessStatusCode();
    }
}

public enum DiscardReason
{
    OrderCancelled = 1
}
