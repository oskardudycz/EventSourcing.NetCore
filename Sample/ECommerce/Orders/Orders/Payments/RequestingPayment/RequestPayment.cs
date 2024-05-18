using Core.Commands;

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

public class HandleRequestPayment(PaymentsApiClient client): ICommandHandler<RequestPayment>
{
    public async Task Handle(RequestPayment command, CancellationToken ct)
    {
       var result =  await client.Request(command, ct);

       result.EnsureSuccessStatusCode();
    }
}
