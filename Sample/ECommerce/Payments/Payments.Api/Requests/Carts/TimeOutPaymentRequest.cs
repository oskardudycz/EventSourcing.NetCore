using System;
using Core.Commands;

namespace Payments.Api.Requests.Carts
{
    public class TimeOutPaymentRequest
    {
        public DateTime TimedOutAt { get; set; }
    }
}
