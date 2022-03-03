namespace Payments.Api.Requests.Carts;

public class RequestPaymentRequest
{
    public Guid OrderId { get; set; }

    public decimal Amount { get; set; }
}