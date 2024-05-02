using ECommerce.Core.Controllers;
using ECommerce.Requests;
using ECommerce.Responses;
using ECommerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService productService): CRUDController(productService)
{
    [HttpPost]
    public Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken ct
    )
    {
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
        return service.GetByIdAsync<ProductDetailsResponse>(id, ct);
    }

    [HttpGet]
    public Task<List<ProductShortInfoResponse>> Get(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    )
    {
        return service.GetPagedAsync<ProductShortInfoResponse>(ct, pageNumber, pageSize);
    }
}
