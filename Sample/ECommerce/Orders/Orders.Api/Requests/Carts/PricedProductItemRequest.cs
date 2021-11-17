using System;

namespace Orders.Api.Requests.Carts;

public class PricedProductItemRequest
{
    public Guid? ProductId { get; set; }

    public int? Quantity { get;  set; }

    public decimal? UnitPrice { get; set; }
}