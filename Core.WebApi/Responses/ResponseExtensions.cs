using Microsoft.AspNetCore.Http;

namespace Core.WebApi.Responses;

public static class ResponseExtensions
{
    public static bool IsSuccessful(this HttpContext context) =>
        context.Response.StatusCode is >= 200 and <= 299;
}
