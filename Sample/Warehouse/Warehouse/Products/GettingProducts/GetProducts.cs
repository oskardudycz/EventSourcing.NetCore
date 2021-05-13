using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Warehouse.Core.Queries;

namespace Warehouse.Products.GettingProducts
{
    internal class HandleGetProducts : IQueryHandler<GetProducts, IReadOnlyList<Product>>
    {
        private const int PageSize = 10;
        private readonly IQueryable<Product> products;

        public HandleGetProducts(IQueryable<Product> products)
        {
            this.products = products;
        }

        public async ValueTask<IReadOnlyList<Product>> Handle(GetProducts query, CancellationToken ct)
        {
            var (filter, page) = query;

            var filteredProducts = string.IsNullOrEmpty(filter)
                ? products
                : products
                    .Where(p =>
                        p.Sku.Value.Contains(query.Filter!) ||
                        p.Name.Contains(query.Filter!) ||
                        p.Description!.Contains(query.Filter!)
                    );

            // await is needed because of https://github.com/dotnet/efcore/issues/21793#issuecomment-667096367
            return await filteredProducts
                .Skip(PageSize * page - 1)
                .Take(PageSize)
                .ToListAsync(ct);
        }
    }

    public record GetProducts
    {
        public string? Filter { get; }

        public int Page { get; }

        private GetProducts(string? filter, int page)
        {
            Filter = filter;
            Page = page;
        }

        public static GetProducts Create(string? filter, int? page)
        {
            page ??= 1;

            if (page <= 0)
                throw new ArgumentOutOfRangeException(nameof(page));

            return new GetProducts(filter, page.Value);
        }

        public void Deconstruct(out string? filter, out int page)
        {
            filter = Filter;
            page = Page;
        }
    };
}
