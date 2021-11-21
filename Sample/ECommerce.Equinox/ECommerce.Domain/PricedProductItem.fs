namespace ECommerce.Domain

open System

type ProductItem = { productId : ProductId; quantity : int }

type PricedProductItem = { productItem : ProductItem; unitPrice : decimal } with

    member x.ProductId = x.productItem.productId
    member x.Quantity = x.productItem.quantity
    member x.TotalPrice = decimal x.Quantity * x.unitPrice

    static member From(productItem, ?unitPrice : decimal) =
        match unitPrice with
        | None -> nullArg (nameof(unitPrice))
        | Some price when price <= 0m -> raise <| ArgumentOutOfRangeException(nameof unitPrice, "Unit price has to be positive number")
        | Some price -> { productItem = productItem; unitPrice = price }

    member x.MatchesProductAndUnitPrice(pricedProductItem : PricedProductItem) =
        x.ProductId = pricedProductItem.ProductId && x.unitPrice = pricedProductItem.unitPrice

    member x.MergeWith(productItem : PricedProductItem) =
        if x.ProductId <> productItem.ProductId then raise <| ArgumentException "Product ids do not match."
        if x.unitPrice <> productItem.unitPrice then raise <| ArgumentException "Product unit prices do not match."
        // TODO fix bug in source: new ProductItem(ProductId, productItem.Quantity + productItem.Quantity),
        { productItem = { productId = x.ProductId; quantity = x.Quantity + productItem.Quantity }; unitPrice = x.unitPrice }
