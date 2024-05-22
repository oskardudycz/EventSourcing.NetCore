using System.Threading.Channels;
using Open.ChannelExtensions;

namespace Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async Task Pipe<T, TResult>(
        this IAsyncEnumerable<T> enumerable,
        ChannelWriter<TResult> cw,
        Func<List<T>, TResult> transform,
        int batchSize,
        int timeout,
        CancellationToken ct
    )
    {
        var channel = Channel.CreateUnbounded<T>(
            new UnboundedChannelOptions
            {
                SingleWriter = false, SingleReader = true, AllowSynchronousContinuations = false
            }
        );

        channel.Reader.Batch(batchSize).WithTimeout(timeout).PipeAsync(async batch =>
        {
            await cw.WriteAsync(transform(batch), ct).ConfigureAwait(false);

            return batch;
        }, cancellationToken: ct);

        await foreach (var @event in enumerable.WithCancellation(ct))
        {
            await channel.Writer.WriteAsync(@event, ct).ConfigureAwait(false);
        }
    }

    public static IAsyncEnumerable<List<T>> Batch<T>(
        this IAsyncEnumerable<T> enumerable,
        int batchSize,
        int timeout,
        CancellationToken ct
    )
    {
        var channel = Channel.CreateUnbounded<T>(
            new UnboundedChannelOptions
            {
                SingleWriter = false, SingleReader = true, AllowSynchronousContinuations = false
            }
        );

        // Start the producer in the background
        _ = ProduceAsync(enumerable, channel, ct).ContinueWith(async t =>
        {
            if (t.IsFaulted)
            {
                await channel.CompleteAsync(t.Exception).ConfigureAwait(false);
            }
        }, ct, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);


        return channel.Reader.Batch(batchSize).WithTimeout(timeout).AsAsyncEnumerable(cancellationToken: ct);
    }


    private static async Task ProduceAsync<T>(this IAsyncEnumerable<T> enumerable, Channel<T> channel, CancellationToken ct)
    {
        await foreach (var @event in enumerable.WithCancellation(ct))
        {
            await channel.Writer.WriteAsync(@event, ct).ConfigureAwait(false);
        }

        await channel.CompleteAsync().ConfigureAwait(false);
    }
}
