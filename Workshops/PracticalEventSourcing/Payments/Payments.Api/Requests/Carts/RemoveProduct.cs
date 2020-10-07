using System;

namespace Carts.Api.Requests.Carts
{
    public class RemoveProductRequest

    {
        public Guid CartId { get; set; }

        public PricedProductItemRequest ProductItem { get; set; }
    }
}
