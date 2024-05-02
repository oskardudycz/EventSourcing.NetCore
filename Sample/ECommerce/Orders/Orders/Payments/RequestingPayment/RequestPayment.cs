using Core.Commands;
using Core.Requests;

namespace Orders.Payments.RequestingPayment;

public record RequestPayment(
    Guid OrderId,
    decimal Amount
)
{
    public static RequestPayment Create(Guid orderId, decimal amount)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        return new RequestPayment(orderId, amount);
    }
}

public class HandleRequestPayment(
    ExternalServicesConfig externalServicesConfig,
    IExternalCommandBus externalCommandBus)
    :
        ICommandHandler<RequestPayment>
{
    public async Task Handle(RequestPayment command, CancellationToken ct)
    {
        await externalCommandBus.Post(
            externalServicesConfig.PaymentsUrl!,
            "payments",
            command,
            ct
        );
    }
}
