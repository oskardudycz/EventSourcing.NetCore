using Payments.Payments.Enums;

namespace Payments.Api.Requests.Carts
{
    public class DiscardPaymentRequest
    {
        public DiscardReason DiscardReason { get; set; }
    }
}
