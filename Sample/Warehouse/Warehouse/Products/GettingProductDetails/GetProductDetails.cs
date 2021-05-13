using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse.Core.Primitives;
using Warehouse.Core.Queries;

namespace Warehouse.Products.GettingProductDetails
{
    internal class HandleGetProductDetails: IQueryHandler<GetProductDetails, Product?>
    {
        private readonly IQueryable<Product> products;

        public HandleGetProductDetails(IQueryable<Product> products)
        {
            this.products = products;
        }

        public async ValueTask<Product?> Handle(GetProductDetails query, CancellationToken ct)
        {
            // await is needed because of https://github.com/dotnet/efcore/issues/21793#issuecomment-667096367
            return await products
                .SingleOrDefaultAsync(p => p.Id == query.ProductId, ct);
        }
    }

    public record GetProductDetails
    {
        public Guid ProductId { get;}

        private GetProductDetails(Guid productId)
        {
            ProductId = productId;
        }

        public static GetProductDetails Create(Guid productId)
            => new(productId.AssertNotEmpty(nameof(productId)));
    }
}
