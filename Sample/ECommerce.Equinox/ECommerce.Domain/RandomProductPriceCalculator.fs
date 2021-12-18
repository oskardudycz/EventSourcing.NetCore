namespace ECommerce.Domain

type RandomProductPriceCalculator() =

    let productPrices = System.Collections.Concurrent.ConcurrentDictionary<ProductId, decimal>()

    member _.Calculate(productId : ProductId) : Async<decimal> = async {
        let r = System.Random()
        let calc _ = (r.NextDouble() |> decimal) * 100m
        let price : decimal = productPrices.GetOrAdd(productId, calc)
        return price
    }
