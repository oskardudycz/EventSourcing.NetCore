using Core.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Core.WebApi.Headers;

public static class ETagExtensions
{
    public static EntityTagHeaderValue? GetIfMatchRequestHeader(this HttpContext context) =>
        context.Request.GetTypedHeaders().IfMatch.FirstOrDefault();

    public static void TrySetETagResponseHeader(this HttpContext context, string etag) =>
        context.Response.TrySetETagResponseHeader(etag);

    public static void TrySetETagResponseHeader(this HttpResponse response, string etag)
    {
        if (!response.IsSuccessful()) return;

        response.GetTypedHeaders().ETag = new EntityTagHeaderValue($"\"{etag}\"", true);
    }

    public static string GetSanitizedValue(this EntityTagHeaderValue eTag)
    {
        var value = eTag.Tag.Value;
        // trim first and last quote characters
        return value.Substring(1, value.Length - 2);
    }
}
