using System;

namespace Carts.Api.Requests.Carts
{
    public class ProductItemRequest
    {
        public Guid ProductId { get; set; }

        public int Quantity { get;  set; }
    }
}
