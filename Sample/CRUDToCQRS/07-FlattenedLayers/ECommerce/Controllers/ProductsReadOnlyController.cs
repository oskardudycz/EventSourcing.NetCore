using ECommerce.Domain.Products;
using ECommerce.Domain.Products.GettingById;
using ECommerce.Domain.Products.GettingProducts;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
public class ProductsReadOnlyController: Controller
{
    private readonly IQueryable<Product> products;

    public ProductsReadOnlyController(IQueryable<Product> products) => this.products = products;

    [HttpGet("api/products/{id:guid}")]
    public Task<ProductDetailsResponse?> GetById([FromRoute] Guid id, CancellationToken ct) =>
        products.HandleAsync(new GetProductById(id), ct);

    [HttpGet("api/products/")]
    public Task<List<ProductShortInfoResponse>> Get(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    ) =>
        products.HandleAsync(new GetProducts(pageNumber, pageSize), ct);
}
