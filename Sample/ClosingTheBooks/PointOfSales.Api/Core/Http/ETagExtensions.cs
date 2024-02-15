using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace PointOfSales.Api.Core;

public static class ETagExtensions
{
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
