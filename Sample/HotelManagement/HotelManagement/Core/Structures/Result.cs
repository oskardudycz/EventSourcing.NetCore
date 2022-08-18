namespace HotelManagement.Core;

public class Result<TSuccess, TError>
{
    public Maybe<TSuccess> Left { get; }
    public Maybe<TError> Right { get; }

    public Result(TSuccess value)
    {
        Left = Maybe<TSuccess>.Of(value);
        Right = Maybe<TError>.Empty;
    }

    public Result(TError value)
    {
        Left = Maybe<TSuccess>.Empty;
        Right = Maybe<TError>.Of(value);
    }

    public Result(Maybe<TSuccess> left, Maybe<TError> right)
    {
        if (!left.IsPresent && !right.IsPresent)
            throw new ArgumentOutOfRangeException(nameof(right));

        Left = left;
        Right = right;
    }

    public TMapped Map<TMapped>(
        Func<TSuccess, TMapped> mapLeft,
        Func<TError, TMapped> mapRight
    )
    {
        if (Left.IsPresent)
            return mapLeft(Left.GetOrThrow());

        if (Right.IsPresent)
            return mapRight(Right.GetOrThrow());

        throw new Exception("That should never happen!");
    }

    public void Switch(
        Action<TSuccess> onLeft,
        Action<TError> onRight
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

public static class Result
{
    public static Result<TSuccess, TError> Success<TSuccess, TError>(TSuccess success) => new(success);

    public static Result<TSuccess, TError> Failure<TSuccess, TError>(TError success) => new(success);
}
