namespace Core.Events;

public interface IAppendScope<TVersion> where TVersion: struct
{
    Task Do(Func<TVersion?, Task<TVersion>> handler);
}

public class AppendScope<TVersion>: IAppendScope<TVersion> where TVersion: struct
{
    private readonly Func<TVersion?> getExpectedVersion;
    private readonly Action<TVersion> setNextExpectedVersion;

    public AppendScope(
        Func<TVersion?> getExpectedVersion,
        Action<TVersion> setNextExpectedVersion
    )
    {
        this.getExpectedVersion = getExpectedVersion;
        this.setNextExpectedVersion = setNextExpectedVersion;
    }

    public async Task Do(Func<TVersion?, Task<TVersion>> handler)
    {
        var nextVersion = await handler(getExpectedVersion()).ConfigureAwait(false);

        setNextExpectedVersion(nextVersion);
    }
}
