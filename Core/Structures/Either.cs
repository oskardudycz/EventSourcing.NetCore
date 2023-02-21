namespace Core.Structures;

public class Either<TLeft, TRight>
{
    public Maybe<TLeft> Left { get; }
    public Maybe<TRight> Right { get; }

    public Either(TLeft value)
    {
        Left = Maybe<TLeft>.Of(value);
        Right = Maybe<TRight>.Empty;
    }

    public Either(TRight value)
    {
        Left = Maybe<TLeft>.Empty;
        Right = Maybe<TRight>.Of(value);
    }

    public Either(Maybe<TLeft> left, Maybe<TRight> right)
    {
        if (!left.IsPresent && !right.IsPresent)
            throw new ArgumentOutOfRangeException(nameof(right));

        Left = left;
        Right = right;
    }

    public TMapped Map<TMapped>(
        Func<TLeft, TMapped> mapLeft,
        Func<TRight, TMapped> mapRight
    )
    {
        if (Left.IsPresent)
            return mapLeft(Left.GetOrThrow());

        if (Right.IsPresent)
            return mapRight(Right.GetOrThrow());

        throw new Exception("That should never happen!");
    }

    public void Switch(
        Action<TLeft> onLeft,
        Action<TRight> onRight
    )
    {
        if (Left.IsPresent)
        {
            onLeft(Left.GetOrThrow());
            return;
        }

        if (Right.IsPresent)
        {
            onRight(Right.GetOrThrow());
            return;
        }

        throw new Exception("That should never happen!");
    }
}

public static class EitherExtensions
{
    public static (TLeft? Left, TRight? Right) AssertAnyDefined<TLeft, TRight>(
        this (TLeft? Left, TRight? Right) value
    )
    {
        if (value.Left == null && value.Right == null)
            throw new ArgumentOutOfRangeException(nameof(value), "One of values needs to be set");

        return value;
    }

    public static TMapped Map<TLeft, TRight, TMapped>(
        this (TLeft? Left, TRight? Right) value,
        Func<TLeft, TMapped> mapLeft,
        Func<TRight, TMapped> mapRight
    )
        where TLeft: struct
        where TRight: struct
    {
        var (left, right) = value.AssertAnyDefined();

        if (left.HasValue)
            return mapLeft(left.Value);

        if (right.HasValue)
            return mapRight(right.Value);

        throw new Exception("That should never happen!");
    }

    public static TMapped Map<TLeft, TRight, TMapped>(
        this (TLeft? Left, TRight? Right) value,
        Func<TLeft, TMapped> mapT1,
        Func<TRight, TMapped> mapT2
    )
    {
        value.AssertAnyDefined();

        var either = value.Left != null
            ? new Either<TLeft, TRight>(value.Left!)
            : new Either<TLeft, TRight>(value.Right!);

        return either.Map(mapT1, mapT2);
    }

    public static void Switch<TLeft, TRight>(
        this (TLeft? Left, TRight? Right) value,
        Action<TLeft> onT1,
        Action<TRight> onT2
    )
    {
        value.AssertAnyDefined();

        var either = value.Left != null
            ? new Either<TLeft, TRight>(value.Left!)
            : new Either<TLeft, TRight>(value.Right!);

        either.Switch(onT1, onT2);
    }

    public static (TLeft?, TRight?) Either<TLeft, TRight>(
        TLeft? left = default
    ) => (left, default);

    public static (TLeft?, TRight?) Either<TLeft, TRight>(
        TRight? right = default
    ) => (default, right);
}
