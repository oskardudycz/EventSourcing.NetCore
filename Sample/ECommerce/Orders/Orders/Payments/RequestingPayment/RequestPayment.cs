using Core.Commands;
using Core.Requests;
using MediatR;

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

public class HandleRequestPayment:
    ICommandHandler<RequestPayment>
{
    private readonly ExternalServicesConfig externalServicesConfig;
    private readonly IExternalCommandBus externalCommandBus;

    public HandleRequestPayment(ExternalServicesConfig externalServicesConfig,
        IExternalCommandBus externalCommandBus)
    {
        this.externalServicesConfig = externalServicesConfig;
        this.externalCommandBus = externalCommandBus;
    }

    public async Task Handle(RequestPayment command, CancellationToken cancellationToken)
    {
        await externalCommandBus.Post(
            externalServicesConfig.PaymentsUrl!,
            "payments",
            command,
            cancellationToken
        );
    }
}
