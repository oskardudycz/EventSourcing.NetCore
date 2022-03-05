using Core.Tracing;

namespace Core.Events;

public interface IAppendScope<TVersion> where TVersion: struct
{
    Task Do(Func<TVersion?, TraceMetadata?, Task<TVersion>> handler);
}

public class AppendScope<TVersion>: IAppendScope<TVersion> where TVersion: struct
{
    private readonly Func<TVersion?> getExpectedVersion;
    private readonly Action<TVersion> setNextExpectedVersion;
    private readonly Func<TraceMetadata?> getEventMetadata;

    public AppendScope(
        Func<TVersion?> getExpectedVersion,
        Action<TVersion> setNextExpectedVersion,
        Func<TraceMetadata?> getEventMetadata
    )
    {
        this.getExpectedVersion = getExpectedVersion;
        this.setNextExpectedVersion = setNextExpectedVersion;
        this.getEventMetadata = getEventMetadata;
    }

    public async Task Do(Func<TVersion?, TraceMetadata?, Task<TVersion>> handler)
    {
        var nextVersion = await handler(getExpectedVersion(), getEventMetadata());

        setNextExpectedVersion(nextVersion);
    }
}
