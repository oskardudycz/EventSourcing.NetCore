using ECommerce.Core.Controllers;
using ECommerce.Domain.Products.Contracts.Requests;
using ECommerce.Domain.Products.Contracts.Responses;
using ECommerce.Domain.Products.Entity;
using ECommerce.Domain.Products.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[Route("api/[controller]")]
public class ProductsController: CRUDController<Product>
{
    protected override Func<object, string> GetEntityByIdUri { get; } = id => $"/api/Products/{id}";

    public ProductsController(ProductService service, ProductReadOnlyService readOnlyService)
        : base(service, readOnlyService)
    {
    }

    [HttpPost]
    public Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken ct
    )
    {
        request.Id = Guid.NewGuid();

        return CreateAsync<CreateProductRequest, ProductDetailsResponse>(
            request,
            ct
        );
    }

    [HttpPut("{id}")]
    public Task<IActionResult> UpdateAsync(
        [FromBody] UpdateProductRequest request,
        CancellationToken ct
    )
    {
        return UpdateAsync<UpdateProductRequest, ProductDetailsResponse>(
            request,
            ct
        );
    }

    [HttpDelete("{id:guid}")]
    public new Task<IActionResult> DeleteByIdAsync([FromRoute] Guid id, CancellationToken ct)
    {
        return base.DeleteByIdAsync(id, ct);
    }

    [HttpGet("{id}")]
    public Task<ProductDetailsResponse> GetById(Guid id, CancellationToken ct)
    {
        return ReadOnlyService.GetByIdAsync<ProductDetailsResponse>(id, ct);
    }

    [HttpGet]
    public Task<List<ProductShortInfoResponse>> Get(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    )
    {
        return ReadOnlyService.GetPagedAsync<ProductShortInfoResponse>(ct, pageNumber, pageSize);
    }
}