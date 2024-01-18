using System.Runtime.CompilerServices;

namespace Core.Validation;

public static class ValidationExtensions
{
    public static T AssertNotNull<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        if (value == null)
            throw new ArgumentNullException(paramName);

        return (T)value;
    }

    public static string AssertNotEmpty(this string? value, [CallerArgumentExpression("value")] string? paramName = null)
        => !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentOutOfRangeException(paramName);

    public static T AssertNotEmpty<T>(this T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
        => AssertNotEmpty((T?)value, paramName);

    public static T AssertNotEmpty<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        var notNullValue = value.AssertNotNull(paramName);

        if (Equals(notNullValue, default(T)))
            throw new ArgumentOutOfRangeException(paramName);

        return notNullValue;
    }

    public static T AssertGreaterOrEqualThan<T>(this T value, T valueToCompare, [CallerArgumentExpression("value")] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(valueToCompare) < 0)
            throw new ArgumentOutOfRangeException(paramName);

        return value;
    }
}
