using KurrentDB.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace OptimisticConcurrency.Core.Http;

public static class ETagExtensions
{
    public static StreamState ToExpectedRevision(string? eTag)
    {
        if (eTag is null)
            throw new ArgumentNullException(nameof(eTag));

        var value = EntityTagHeaderValue.Parse(eTag).Tag.Value;

        if (value is null)
            throw new ArgumentNullException(nameof(eTag));

        var version = long.Parse(value.Substring(1, value.Length - 2));

        return version >= 0 ? StreamState.StreamRevision((ulong)version) : StreamState.NoStream;
    }

    public static void SetResponseEtag(this HttpContext httpContext, StreamState? revision)
    {
        if (!revision.HasValue)
            return;

        httpContext.Response.Headers.ETag = revision.Value.ToInt64().ToWeakEtag();
    }

    public static StringValues ToWeakEtag(this long version) =>
        $"W/\"{version}\"";
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromIfMatchHeaderAttribute: FromHeaderAttribute
{
    public FromIfMatchHeaderAttribute() =>
        Name = "If-Match";
}
