using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Marten.Pagination;
using Newtonsoft.Json;

namespace Tickets.Api.Responses
{
    public class PagedListResponse<T>
    {
        public IReadOnlyList<T> Items { get; }

        public long TotalItemCount { get; }

        public bool HasNextPage { get; }

        [JsonConstructor]
        internal PagedListResponse(IEnumerable<T> items, long totalItemCount, bool hasNextPage)
        {
            Items = items.ToList();
            TotalItemCount = totalItemCount;
            HasNextPage = hasNextPage;
        }
    }

    public static class PagedListResponse
    {
        public static PagedListResponse<T> From<T>(IPagedList<T> pagedList)
        {
            Guard.Against.Null(pagedList, nameof(PagedListResponse<T>.Items));
            Guard.Against.Negative(pagedList.TotalItemCount, nameof(PagedListResponse<T>.TotalItemCount));

            return new PagedListResponse<T>(pagedList, pagedList.TotalItemCount, pagedList.HasNextPage);
        }
    }
}
