using System.Threading.Channels;
using Core.Events;

namespace Core.Testing;

public class EventListener
{
    private readonly Channel<object> events = Channel.CreateUnbounded<object>(
        new UnboundedChannelOptions { SingleWriter = true, AllowSynchronousContinuations = false }
    );

    public ChannelReader<object> Reader => events.Reader;
    public ChannelWriter<object> Writer => events.Writer;

    public async Task Handle(object @event, CancellationToken ct)
    {
        await events.Writer.WriteAsync(@event, ct).ConfigureAwait(false);
    }

    public async Task<object> WaitForProcessing(object @event, CancellationToken ct)
    {
        await foreach (var item in events.Reader.ReadAllAsync(ct).ConfigureAwait(false))
        {
            if (item.Equals(@event))
                return item;
        }

        throw new Exception("No events were found");
    }

    public IAsyncEnumerable<object> ReadAll(CancellationToken ct) =>
        events.Reader.ReadAllAsync(ct);
}

public class EventCatcher: IEventBus
{
    private readonly EventListener listener;
    private readonly IEventBus eventBus;

    public EventCatcher(EventListener listener, IEventBus eventBus)
    {
        this.listener = listener;
        this.eventBus = eventBus;
    }

    public async Task Publish(IEventEnvelope @event, CancellationToken ct)
    {
        await eventBus.Publish(@event, ct).ConfigureAwait(false);

        await listener.Handle(@event, ct).ConfigureAwait(false);
        await listener.Handle(@event.Data, ct).ConfigureAwait(false);
    }
}

