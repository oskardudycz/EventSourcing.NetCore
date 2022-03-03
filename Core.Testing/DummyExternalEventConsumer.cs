using Core.Events.External;

namespace Core.Testing;

public class DummyExternalEventConsumer: IExternalEventConsumer
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}