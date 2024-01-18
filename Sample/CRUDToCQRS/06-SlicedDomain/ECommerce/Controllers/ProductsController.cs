using ECommerce.Contracts.Requests;
using ECommerce.Core.Controllers;
using ECommerce.Domain.Products;
using ECommerce.Domain.Products.GettingById;
using ECommerce.Domain.Products.GettingProducts;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController: Controller
{
    private readonly ProductService service;
    private readonly ProductReadOnlyService readOnlyService;

    public ProductsController(ProductService service, ProductReadOnlyService readOnlyService)
    {
        this.service = service;
        this.readOnlyService = readOnlyService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken ct
    )
    {
        var command = request.ToCommand(Guid.NewGuid());

        await service.CreateAsync(command, ct);

        return this.Created(command.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct
    )
    {
        var command = request.ToCommand(Guid.NewGuid());

        await service.UpdateAsync(command, ct);

        return this.OkWithLocation(id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute] Guid id, CancellationToken ct)
    {
        await service.DeleteByIdAsync(id, ct);

        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public Task<ProductDetailsResponse?> GetById([FromRoute] Guid id, CancellationToken ct) =>
        readOnlyService.GetByIdAsync(new GetProductById(id), ct);

    [HttpGet]
    public Task<List<ProductShortInfoResponse>> Get(
        CancellationToken ct,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20
    ) =>
        readOnlyService.GetPagedAsync(new GetProducts(pageNumber, pageSize), ct);
}
