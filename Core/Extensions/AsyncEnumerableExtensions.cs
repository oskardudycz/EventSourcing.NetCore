using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T[]> BatchAsync<T>(
        this IAsyncEnumerable<T> source,
        int batchSize,
        TimeSpan maxBatchTime,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<T>();
        var stopwatch = new Stopwatch();

        try
        {
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                batch.Add(item);
                if (batch.Count == 1)
                    stopwatch.Start(); // Start the stopwatch when the first item is added to the batch

                if (batch.Count >= batchSize || stopwatch.Elapsed >= maxBatchTime)
                {
                    yield return batch.ToArray();  // Yield the current batch
                    batch.Clear();                // Clear the batch
                    stopwatch.Restart();          // Restart the stopwatch
                }
            }

            if (batch.Count > 0)
            {
                yield return batch.ToArray(); // Yield any remaining items in the batch
            }
        }
        finally
        {
            stopwatch.Stop();  // Stop the stopwatch
        }
    }
}
