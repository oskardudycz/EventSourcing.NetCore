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

public class EventCatcher<T>: IEventHandler<T>
{
    private readonly EventListener listener;

    public EventCatcher(EventListener listener) =>
        this.listener = listener;

    public Task Handle(T @event, CancellationToken ct)
    {
        return listener.Handle(@event!, ct);
    }
}

