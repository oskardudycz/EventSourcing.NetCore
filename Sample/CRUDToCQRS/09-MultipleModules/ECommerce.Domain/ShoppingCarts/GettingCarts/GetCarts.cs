namespace ECommerce.Domain.ShoppingCarts.GettingCarts;

public record GetCarts(
    int PageNumber = 1,
    int PageSize = 20
);

public class ShoppingCartShortInfo
{
    public Guid Id { get; set; }

    public int TotalItemsCount { get; set; }

    public ShoppingCartStatus Status { get; set; }
}
