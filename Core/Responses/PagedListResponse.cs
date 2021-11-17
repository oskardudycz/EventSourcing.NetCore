using System.Collections.Generic;
using System.Linq;

namespace Core.Responses;

public class PagedListResponse<T>
{
    public IReadOnlyList<T> Items { get; }

    public long TotalItemCount { get; }

    public bool HasNextPage { get; }

    public PagedListResponse(IEnumerable<T> items, long totalItemCount, bool hasNextPage)
    {
        Items = items.ToList();
        TotalItemCount = totalItemCount;
        HasNextPage = hasNextPage;
    }
}