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

public class HandleDiscardPayment:
    ICommandHandler<DiscardPayment>
{
    private readonly ExternalServicesConfig externalServicesConfig;
    private readonly IExternalCommandBus externalCommandBus;

    public HandleDiscardPayment(ExternalServicesConfig externalServicesConfig,
        IExternalCommandBus externalCommandBus)
    {
        this.externalServicesConfig = externalServicesConfig;
        this.externalCommandBus = externalCommandBus;
    }

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
