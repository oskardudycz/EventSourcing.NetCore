using ECommerce.Core.Controllers;
using ECommerce.Domain.Products.Contracts.Requests;
using ECommerce.Domain.Products.Contracts.Responses;
using ECommerce.Domain.Products.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService service, ProductReadOnlyService readOnlyService)
    : Controller
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken ct
    )
    {
        request.Id = Guid.NewGuid();
        await service.CreateAsync(request, ct);

        return this.Created(request.Id);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsync(
        Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct
    )
    {
        request.Id = id;
        await service.UpdateAsync(request, ct);

        return this.OkWithLocation(id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute] Guid id, CancellationToken ct)
    {
        await service.DeleteByIdAsync(id, ct);

        return NoContent();
    }

    [HttpGet("{id}")]
    public Task<ProductDetailsResponse?> GetById(Guid id, CancellationToken ct)
    {
        return readOnlyService.GetByIdAsync(id, ct);
    }

    [HttpGet]
    public Task<List<ProductShortInfoResponse>> Get(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    )
    {
        return readOnlyService.GetPagedAsync(ct, pageNumber, pageSize);
    }
}
