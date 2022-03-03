namespace Core.Events.External;

public interface IExternalEventConsumer
{
    Task StartAsync(CancellationToken cancellationToken);
}