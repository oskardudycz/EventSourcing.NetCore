namespace ECommerce.Domain

type IProductPriceCalculator =

    abstract Calculate : ProductItem -> PricedProductItem

type RandomProductPriceCalculator() =

    let productPrices = System.Collections.Concurrent.ConcurrentDictionary<ProductId, decimal>()

    interface IProductPriceCalculator with
        override _.Calculate(productItem : ProductItem) =
            let r = System.Random()
            let calc _ = (r.NextDouble() |> decimal) * 100m
            let price : decimal = productPrices.GetOrAdd(productItem.productId, calc)
            PricedProductItem.From(productItem, price)
