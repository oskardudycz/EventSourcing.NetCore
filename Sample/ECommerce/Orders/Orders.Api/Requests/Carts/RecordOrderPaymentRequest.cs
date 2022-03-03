namespace Orders.Api.Requests.Carts;

public class RecordOrderPaymentRequest
{
    public Guid PaymentId { get; set; }

    public DateTime PaymentRecordedAt { get; set; }
}