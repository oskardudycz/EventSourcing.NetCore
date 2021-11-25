namespace ECommerce.Domain

type IProductPriceCalculator =

    abstract Calculate : id : ProductId -> decimal

type RandomProductPriceCalculator() =

    let productPrices = System.Collections.Concurrent.ConcurrentDictionary<ProductId, decimal>()

    interface IProductPriceCalculator with

        override _.Calculate(productId : ProductId) =
            let r = System.Random()
            let calc _ = (r.NextDouble() |> decimal) * 100m
            let price : decimal = productPrices.GetOrAdd(productId, calc)
            price
