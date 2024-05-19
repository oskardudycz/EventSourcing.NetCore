namespace MarketBasketAnalytics.MarketBasketAnalysis
{
    public record ProductRelationshipsInBaskets(
        IReadOnlyList<Guid> RelatedProducts,
        int BasketWithProductsCount
    );

    public record ProductRelationshipsCalculated(
        Guid ProductId,
        IReadOnlyList<ProductRelationshipsInBaskets> Relationships,
        int BasketsCount
    )
    {
        public static ProductRelationshipsCalculated Default() =>
            new(default, Array.Empty<ProductRelationshipsInBaskets>(), default);
    }

    public static class ProductRelationships
    {
        public static async Task<ProductRelationshipsCalculated> Handle(
            Func<string, CancellationToken, Task<ProductRelationshipsCalculated?>> getCurrentSummary,
            CartProductItemsMatched @event,
            CancellationToken ct
        )
        {
            var result = new List<ProductRelationshipsInBaskets>();

            var currentSummary = await getCurrentSummary(ToStreamId(@event.ProductId), ct)
                ?? ProductRelationshipsCalculated.Default();
            
            var relatedProducts = Expand(@event.RelatedProducts).ToList();

            foreach (var currentRel in currentSummary.Relationships)
            {
                var relationship = relatedProducts
                    .SingleOrDefault(rp => rp.SequenceEqual(currentRel.RelatedProducts));

                if (relationship == null)
                {
                    result.Add(currentRel);
                    continue;
                }

                result.Add(new ProductRelationshipsInBaskets(
                    relationship,
                    currentRel.BasketWithProductsCount + 1
                ));
                relatedProducts.Remove(relationship);
            }

            result.AddRange(
                relatedProducts
                    .Select(relationship =>
                        new ProductRelationshipsInBaskets(relationship, 1)
                    ).ToList()
            );

            return currentSummary with { Relationships = result, BasketsCount = currentSummary.BasketsCount + 1 };
        }

        private static IReadOnlyList<IReadOnlyList<Guid>> Expand(IReadOnlyList<Guid> relatedProducts)
            => relatedProducts
                .SelectMany(
                    (relatedProduct, index) =>
                        Expand(new[] { relatedProduct }, relatedProducts.Skip(index + 1).ToList())
                )
                .ToList();

        private static IEnumerable<IReadOnlyList<Guid>> Expand
        (
            IReadOnlyList<Guid> accumulator,
            IReadOnlyList<Guid> relatedProducts
        )
        {
            if (!relatedProducts.Any())
                return new[] { accumulator };

            var aggregates = relatedProducts
                .Select(relatedProduct => accumulator.Union(new[] { relatedProduct }).ToList())
                .ToList();

            return aggregates.Union(
                aggregates.SelectMany((acc, i) => Expand(acc, relatedProducts.Skip(i + 1).ToList()))
                    .ToList()
            );
        }

        public static string ToStreamId(Guid shoppingCartId) =>
            $"cart_product_items_matching-{shoppingCartId}";
    }
}
