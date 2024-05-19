using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Core.Controllers;

public static class HttpExtensions
{
    public static CreatedResult Created(this Controller controller, Guid id) =>
        new($"api/{controller.ControllerContext.ActionDescriptor.ControllerName}/{id}", id);

    public static OkResult OkWithLocation(this Controller controller, string location)
    {
        controller.Response.Headers.Location = location;

        return new OkResult();
    }

    public static OkResult OkWithLocation(this Controller controller, Guid id) =>
        controller.OkWithLocation($"api/{controller.ControllerContext.ActionDescriptor.ControllerName}/{id}");
}
