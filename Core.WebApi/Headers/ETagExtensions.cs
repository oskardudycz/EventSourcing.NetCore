using Core.WebApi.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Core.WebApi.Headers;

public static class ETagExtensions
{
    public static EntityTagHeaderValue? GetIfMatchRequestHeader(this HttpContext context) =>
        context.Request.GetTypedHeaders().IfMatch.FirstOrDefault();

    public static void TrySetETagResponseHeader(this HttpContext context, object etag) =>
        context.Response.TrySetETagResponseHeader(etag);

    public static void TrySetETagResponseHeader(this HttpResponse response, object etag)
    {
        if (!response.IsSuccessful()) return;

        response.GetTypedHeaders().ETag = new EntityTagHeaderValue($"\"{etag}\"", true);
    }

    public static string GetSanitizedValue(this EntityTagHeaderValue eTag)
    {
        var value = eTag.Tag.Value;

        if (value is null)
            throw new ArgumentNullException(nameof(eTag));
        // trim first and last quote characters
        return value.Substring(1, value.Length - 2);
    }

    public static int ToExpectedVersion(string? eTag)
    {
        if (eTag is null)
            throw new ArgumentNullException(nameof(eTag));

        var value = EntityTagHeaderValue.Parse(eTag).Tag.Value;

        if (value is null)
            throw new ArgumentNullException(nameof(eTag));

        return int.Parse(value.Substring(1, value.Length - 2));
    }
}


[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromIfMatchHeaderAttribute: FromHeaderAttribute
{
    public FromIfMatchHeaderAttribute()
    {
        Name = "If-Match";
    }
}
