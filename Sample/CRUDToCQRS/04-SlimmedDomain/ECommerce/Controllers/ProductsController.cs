using ECommerce.Core.Controllers;
using ECommerce.Domain.Products.Contracts.Requests;
using ECommerce.Domain.Products.Contracts.Responses;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Products.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService service, ProductReadOnlyService readOnlyService)
    : CRUDController<Product>(service, readOnlyService)
{
    protected override Func<object, string> GetEntityByIdUri { get; } = id => $"/api/Products/{id}";

    [HttpPost]
    public Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken ct
    )
    {
        request.Id = Guid.CreateVersion7();

        return base.CreateAsync(request, ct);
    }

    [HttpPut("{id:guid}")]
    public Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct
    )
    {
        request.Id = id;
        return base.UpdateAsync(request, ct);
    }

    [HttpDelete("{id:guid}")]
    public new Task<IActionResult> DeleteByIdAsync([FromRoute] Guid id, CancellationToken ct) =>
        base.DeleteByIdAsync(id, ct);

    [HttpGet("{id}")]
    public Task<ProductDetailsResponse> GetById(Guid id, CancellationToken ct) =>
        ReadOnlyService.GetByIdAsync<ProductDetailsResponse>(id, ct);

    [HttpGet]
    public Task<List<ProductShortInfoResponse>> Get(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    ) =>
        ReadOnlyService.GetPagedAsync<ProductShortInfoResponse>(ct, pageNumber, pageSize);
}
