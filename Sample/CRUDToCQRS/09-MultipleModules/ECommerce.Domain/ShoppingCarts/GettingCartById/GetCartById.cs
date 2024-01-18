using ECommerce.Domain.ShoppingCarts.Products;

namespace ECommerce.Domain.ShoppingCarts.GettingCartById;

public record GetCartById(
    Guid CartId
);

public class ShoppingCartDetails
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }

    public ShoppingCartStatus Status { get; set; }

    public IList<PricedProductItem> ProductItems { get; set; } = default!;

    public decimal TotalPrice => ProductItems.Sum(pi => pi.TotalPrice);

    public int Version { get; set; }
}

