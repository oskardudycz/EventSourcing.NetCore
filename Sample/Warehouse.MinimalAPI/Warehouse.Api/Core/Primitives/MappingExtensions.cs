using System;

namespace Warehouse.Api.Core.Primitives;

internal static class MappingExtensions
{
    public static T AssertNotNull<T>(this T? value, string? paramName = null)
        where T : struct
    {
        if (value == null)
            throw new ArgumentNullException(paramName);

        return (T)value;
    }

    public static string AssertNotEmpty(this string? value, string? paramName = null)
        => !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentOutOfRangeException(paramName);

    public static T AssertNotEmpty<T>(this T value, string? paramName = null)
        where T : struct
        => AssertNotEmpty((T?)value, paramName);

    public static T AssertNotEmpty<T>(this T? value, string? paramName = null)
        where T : struct
    {
        var notNullValue = value.AssertNotNull(paramName);

        if(Equals(notNullValue, default(T)))
            throw new ArgumentOutOfRangeException(paramName);

        return notNullValue;
    }
}