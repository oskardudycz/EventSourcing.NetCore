using System.Runtime.CompilerServices;

namespace Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T[]> BatchAsync<T>(
        this IAsyncEnumerable<T> source,
        int batchSize,
        [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        var batch = new List<T>(batchSize);

        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            batch.Add(item);

            if (batch.Count >= batchSize)
            {
                yield return batch.ToArray();

                batch.Clear();
            }
        }

        if (batch.Count > 0)
            yield return batch.ToArray();
    }
}
