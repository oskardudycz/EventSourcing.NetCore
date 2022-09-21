using Microsoft.AspNetCore.Http;
using static Microsoft.AspNetCore.Http.Results;

namespace ECommerce.Domain.Core.Http;

public static class HttpExtensions
{
    public static IResult OkWithLocation(HttpContext httpContext, string location)
    {
        httpContext.Response.Headers.Location = location;

        return Ok();
    }
}
