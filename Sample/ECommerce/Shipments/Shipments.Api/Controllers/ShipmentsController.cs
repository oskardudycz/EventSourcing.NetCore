using Microsoft.AspNetCore.Mvc;
using Shipments.Packages;
using Shipments.Packages.Requests;

namespace Shipments.Api.Controllers;

[Route("api/[controller]")]
public class ShipmentsController: Controller
{
    private readonly IPackageService packageService;

    public ShipmentsController(IPackageService packageService)
    {
        this.packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] SendPackage? request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var package = await packageService.SendPackage(request, HttpContext.RequestAborted);

        return Created("api/Reservations", package.Id);
    }

    [HttpGet("{id}")]
    public Task<Package> Get(Guid id)
    {
        return packageService.GetById(id, HttpContext.RequestAborted);
    }
}