using ECommerce.Contracts.Requests;
using ECommerce.Core.Controllers;
using ECommerce.Domain.Core.Repositories;
using ECommerce.Domain.Products;
using ECommerce.Domain.Products.CreatingProduct;
using ECommerce.Domain.Products.GettingById;
using ECommerce.Domain.Products.GettingProducts;
using ECommerce.Domain.Products.UpdatingProduct;
using ECommerce.Domain.Storage;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Controllers;

using static CreateProductHandler;
using static UpdateProductHandler;

[ApiController]
[Route("api/[controller]")]
public class ProductsController: Controller
{
    private readonly ECommerceDbContext dbContext;

    public ProductsController(ECommerceDbContext dbContext) =>
        this.dbContext = dbContext;

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateProductRequest request,
        CancellationToken ct
    )
    {
        var command = request.ToCommand(Guid.NewGuid());

        await dbContext.AddAndSaveChanges(
            Handle(command),
            ct
        );

        return this.Created(command.Id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateProductRequest request,
        CancellationToken ct
    )
    {
        var command = request.ToCommand(id);

        await dbContext.UpdateAndSaveChanges<Product>(
            command.Id,
            product => Handle(product, command),
            ct
        );

        return this.OkWithLocation(id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteByIdAsync([FromRoute] Guid id, CancellationToken ct)
    {
        await dbContext.DeleteAndSaveChanges<Product>(id, ct);

        return NoContent();
    }
}
