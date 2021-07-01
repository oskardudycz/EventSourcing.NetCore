using Ardalis.GuardClauses;
using Carts.Carts.Projections;
using Core.Queries;
using Marten.Pagination;

namespace Carts.Carts.Queries
{
    public class GetCarts : IQuery<IPagedList<CartShortInfo>>
    {
        public int PageNumber { get; }
        public int PageSize { get; }

        private GetCarts(int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        public static GetCarts Create(int pageNumber = 1, int pageSize = 20)
        {
            Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
            Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));

            return new GetCarts(pageNumber, pageSize);
        }
    }
}
