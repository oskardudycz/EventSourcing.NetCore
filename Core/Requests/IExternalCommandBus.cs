namespace Core.Requests;

public interface IExternalCommandBus
{
    Task Send<T>(T command, CancellationToken ct = default) where T : notnull;
}
