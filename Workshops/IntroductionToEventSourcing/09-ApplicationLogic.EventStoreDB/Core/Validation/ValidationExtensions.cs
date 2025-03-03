using System.Numerics;
using System.Runtime.CompilerServices;

namespace Core.Validation;

public static class ValidationExtensions
{
    public static T NotNull<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        if (value == null)
            throw new ArgumentNullException(paramName);

        return value.Value;
    }

    public static T NotNull<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : class
    {
        if (value == null)
            throw new ArgumentNullException(paramName);

        return value;
    }

    public static string NotEmpty(this string? value, [CallerArgumentExpression("value")] string? paramName = null)
        => !string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentOutOfRangeException(paramName);

    public static Guid NotEmpty(this Guid? value, [CallerArgumentExpression("value")] string? paramName = null)
        => value!= null && value != Guid.Empty ? value.Value : throw new ArgumentOutOfRangeException(paramName);


    public static T NotEmpty<T>(this T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
        => NotEmpty((T?)value, paramName);

    public static T Has<T>(this T value, Action<T> assert)
    {
        assert(value);
        return value;
    }

    public static T NotEmpty<T>(this T? value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : struct
    {
        var notNullValue = value.NotNull(paramName);

        if (Equals(notNullValue, default(T)))
            throw new ArgumentOutOfRangeException(paramName);

        return notNullValue;
    }

    public static T GreaterOrEqualThan<T>(this T value, T valueToCompare, [CallerArgumentExpression("value")] string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(valueToCompare) < 0)
            throw new ArgumentOutOfRangeException(paramName);

        return value;

    }

    public static T Positive<T>(this T value, [CallerArgumentExpression("value")] string? paramName = null)
        where T : INumber<T>
    {
        if (value == null || value.CompareTo(Convert.ChangeType(0, typeof(T))) <= 0)
            throw new ArgumentOutOfRangeException(paramName);

        return value;
    }
}
