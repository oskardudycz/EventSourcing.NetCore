namespace ECommerce.Domain

open FSharp.UMX
open System

type ProductId = Guid<productId>
and [<Measure>] productId
module ProductId =
    let toString (x : ProductId) : string = (UMX.untag x).ToString("N")
    let parse (value : Guid) : ProductId = %value
    let (|Parse|) = parse

type ClientId = Guid<clientId>
and [<Measure>] clientId
module ClientId =
    let toString (x : ClientId) : string = (UMX.untag x).ToString("N")
    let parse (value : Guid Nullable) : ClientId =
        if not value.HasValue || value.Value = Guid.Empty then raise <| ArgumentOutOfRangeException(nameof value)
        %value.Value
    let (|Parse|) = parse

type CartId = Guid<cartId>
and [<Measure>] cartId
module CartId =
    let toString (x : CartId) : string = (UMX.untag x).ToString("N")
    let parse (value : Guid Nullable) : CartId =
        if not value.HasValue || value.Value = Guid.Empty then raise <| ArgumentOutOfRangeException(nameof value)
        %value.Value
    let (|Parse|) = parse
    let generate () : CartId = Guid.NewGuid() |> Nullable |> parse
