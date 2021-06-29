// using System;
// using Carts.Carts.Events;
// using Marten.Events;
// using Marten.Events.Projections;
//
// namespace Carts.Carts.Projections
// {
//     public record CartHistory (
//         Guid Id,
//         Guid CartId,
//         string Description
//     );
//
//     public class CartHistoryTransformation : EventProjection
//     {
//         public CartHistory Transform(IEvent<CartInitialized> input)
//         {
//             return new (
//                 Guid.NewGuid(),
//                 input.Data.CartId,
//                 $"Created tentative Cart with id {input.Data.CartId}"
//             );
//         }
//
//         public CartHistory Transform(IEvent<ProductAdded> input)
//         {
//             return new (
//                 Guid.NewGuid(),
//                 input.Data.CartId,
//                 $"Added {input.Data.ProductItem.Quantity} Product with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
//             );
//         }
//
//         public CartHistory Transform(IEvent<ProductRemoved> input)
//         {
//             return new (
//                 Guid.NewGuid(),
//                 input.Data.CartId,
//                 $"Removed Product {input.Data.ProductItem.Quantity} with id `{input.Data.ProductItem.ProductId}` to Cart `{input.Data.CartId}`"
//             );
//         }
//
//         public CartHistory Transform(IEvent<CartConfirmed> input)
//         {
//             return new (
//                 Guid.NewGuid(),
//                 input.Data.CartId,
//                 $"Confirmed Cart with id `{input.Data.CartId}`"
//             );
//         }
//     }
// }
