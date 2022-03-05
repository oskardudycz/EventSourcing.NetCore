using Core.Responses;
using Marten.Pagination;

namespace Core.Marten.Responses;

public static class PagedListExtensions
{
    public static PagedListResponse<T> ToResponse<T>(this IPagedList<T> pagedList) =>
        new(pagedList, pagedList.TotalItemCount, pagedList.HasNextPage);
}
