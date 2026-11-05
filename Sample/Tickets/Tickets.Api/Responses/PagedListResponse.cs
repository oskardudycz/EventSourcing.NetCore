using Marten.Pagination;
using System.Text.Json.Serialization;

namespace Tickets.Api.Responses;

public class PagedListResponse<T>
{
    public IReadOnlyList<T> Items { get; }

    public long TotalItemCount { get; }

    public bool HasNextPage { get; }

    [JsonConstructor]
    internal PagedListResponse(IReadOnlyList<T> items, long totalItemCount, bool hasNextPage)
    {
        Items = items.ToList();
        TotalItemCount = totalItemCount;
        HasNextPage = hasNextPage;
    }
}

public static class PagedListResponse
{
    public static PagedListResponse<T> From<T>(IPagedList<T> pagedList) =>
        new(pagedList.ToArray(), pagedList.TotalItemCount, pagedList.HasNextPage);
}
