using System;
using Carts.Carts.ValueObjects;

namespace Carts.Api.Requests.Carts
{
    public class AddProductRequest
    {
        public Guid CartId { get; set; }

        public ProductItemRequest ProductItem { get; set; }
    }
}
