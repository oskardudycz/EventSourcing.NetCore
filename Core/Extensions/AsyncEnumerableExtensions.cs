using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Core.Extensions;

public static class AsyncEnumerableExtensions
{
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T[]> BatchAsync<T>(
        this IAsyncEnumerable<T> source,
        int batchSize,
        TimeSpan maxBatchTime,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var batch = new List<T>();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            await foreach (var item in source.WithCancellation(cts.Token).ConfigureAwait(false))
            {
                batch.Add(item);
                if (batch.Count == 1) // Reset the timer when the first item is added
                {
                    cts.CancelAfter(maxBatchTime); // Set or reset the deadline
                }

                if (batch.Count >= batchSize)
                {
                    yield return batch.ToArray();
                    batch.Clear();
                    cts.CancelAfter(maxBatchTime); // Reset the deadline for the new batch
                }
            }

            if (batch.Count > 0)
            {
                yield return batch.ToArray(); // Return any remaining items as a batch
            }
        }
        catch (OperationCanceledException)
        {
            if (batch.Count > 0)
            {
                yield return batch.ToArray(); // Yield whatever is in the batch when the timeout occurs
            }
            // Optionally, rethrow or handle the cancellation if needed
        }
        finally
        {
            cts.Dispose(); // Ensure the CancellationTokenSource is disposed to free resources
        }
    }
}
