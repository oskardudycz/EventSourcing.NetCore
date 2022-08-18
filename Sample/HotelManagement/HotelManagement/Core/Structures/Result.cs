namespace HotelManagement.Core.Structures;

public class Result<TSuccess, TError>
{
    public Maybe<TSuccess> Success { get; }
    public Maybe<TError> Error { get; }

    public Result(TSuccess value)
    {
        Success = Maybe<TSuccess>.Of(value);
        Error = Maybe<TError>.Empty;
    }

    public Result(TError value)
    {
        Success = Maybe<TSuccess>.Empty;
        Error = Maybe<TError>.Of(value);
    }

    public Result(Maybe<TSuccess> success, Maybe<TError> error)
    {
        if (!success.IsPresent && !error.IsPresent)
            throw new ArgumentOutOfRangeException(nameof(error));

        Success = success;
        Error = error;
    }

    public TMapped Map<TMapped>(
        Func<TSuccess, TMapped> mapSuccess,
        Func<TError, TMapped> mapFailure
    )
    {
        if (Success.IsPresent)
            return mapSuccess(Success.GetOrThrow());

        if (Error.IsPresent)
            return mapFailure(Error.GetOrThrow());

        throw new Exception("That should never happen!");
    }

    public void Switch(
        Action<TSuccess> onSuccess,
        Action<TError> onFailure
    )
    {
        if (Success.IsPresent)
        {
            onSuccess(Success.GetOrThrow());
            return;
        }

        if (Error.IsPresent)
        {
            onFailure(Error.GetOrThrow());
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
