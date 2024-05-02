namespace Core.Responses;

public class PagedListResponse<T>(IEnumerable<T> items, long totalItemCount, bool hasNextPage)
{
    public IReadOnlyList<T> Items { get; } = items.ToList();

    public long TotalItemCount { get; } = totalItemCount;

    public bool HasNextPage { get; } = hasNextPage;
}
