// using System;
// using Ardalis.GuardClauses;
// using Carts.Carts.Projections;
// using Core.Queries;
// using Marten.Pagination;
//
// namespace Carts.Carts.Queries
// {
//     public class GetCartHistory : IQuery<IPagedList<CartHistory>>
//     {
//         public Guid CartId { get; }
//         public int PageNumber { get; }
//         public int PageSize { get; }
//
//         private GetCartHistory(Guid cartId, int pageNumber, int pageSize)
//         {
//             CartId = cartId;
//             PageNumber = pageNumber;
//             PageSize = pageSize;
//         }
//
//         public static GetCartHistory Create(Guid cartId,int pageNumber = 1, int pageSize = 20)
//         {
//             Guard.Against.NegativeOrZero(pageNumber, nameof(pageNumber));
//             Guard.Against.NegativeOrZero(pageSize, nameof(pageSize));
//
//             return new GetCartHistory(cartId, pageNumber, pageSize);
//         }
//     }
// }
