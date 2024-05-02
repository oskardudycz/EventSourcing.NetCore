using Core.Commands;
using Core.Requests;

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

public class HandleDiscardPayment(
    ExternalServicesConfig externalServicesConfig,
    IExternalCommandBus externalCommandBus)
    :
        ICommandHandler<DiscardPayment>
{
    public async Task Handle(DiscardPayment command, CancellationToken ct)
    {
        await externalCommandBus.Delete(
            externalServicesConfig.PaymentsUrl!,
            "payments",
            command,
            ct
        );
    }
}

public enum DiscardReason
{
    OrderCancelled = 1
}
