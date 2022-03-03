using Core.Commands;
using Core.Requests;
using MediatR;

namespace Orders.Payments.DiscardingPayment;

public class DiscardPayment: ICommand
{
    public Guid PaymentId { get; }

    public DiscardReason DiscardReason { get; }

    private DiscardPayment(Guid paymentId, DiscardReason discardReason)
    {
        PaymentId = paymentId;
        DiscardReason = discardReason;
    }

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

    public async Task<Unit> Handle(DiscardPayment command, CancellationToken cancellationToken)
    {
        await externalCommandBus.Delete(
            externalServicesConfig.PaymentsUrl!,
            "payments",
            command,
            cancellationToken);

        return Unit.Value;
    }
}