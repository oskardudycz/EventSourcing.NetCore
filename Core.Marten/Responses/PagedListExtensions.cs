using Ardalis.GuardClauses;
using Core.Responses;
using Marten.Pagination;

namespace Core.Marten.Responses
{
    public static class PagedListExtensions
    {
        public static PagedListResponse<T> ToResponse<T>(this IPagedList<T> pagedList)
        {
            Guard.Against.Null(pagedList, nameof(PagedListResponse<T>.Items));
            Guard.Against.Negative(pagedList.TotalItemCount, nameof(PagedListResponse<T>.TotalItemCount));

            return new PagedListResponse<T>(pagedList, pagedList.TotalItemCount, pagedList.HasNextPage);
        }
    }
}
