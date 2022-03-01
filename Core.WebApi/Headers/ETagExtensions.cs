using System.Linq;
using Core.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Core.WebApi.Headers;

public static class ETagExtensions
{
    public static EntityTagHeaderValue? GetIfMatchRequestHeader(this HttpContext context) =>
        context.Request.GetTypedHeaders().IfMatch.FirstOrDefault();

    public static void TrySetETagResponseHeader(HttpContext context, string etag)
    {
        if (!context.IsSuccessful()) return;

        context.Response.GetTypedHeaders().ETag = new EntityTagHeaderValue(etag, true);
    }
}
