using Payments.Payments.DiscardingPayment;

namespace Payments.Api.Requests.Carts
{
    public class DiscardPaymentRequest
    {
        public DiscardReason DiscardReason { get; set; }
    }
}
