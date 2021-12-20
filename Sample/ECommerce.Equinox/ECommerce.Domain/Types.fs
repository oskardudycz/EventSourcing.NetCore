namespace ECommerce.Domain

open FSharp.UMX
open System

module Guid =

    let inline toStringN (x : Guid) = x.ToString "N"

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

(* At present, there's only a single series of Confirmed carts; this extension point could be used
   e.g. if one was to implement multi-tenancy, the tenantId would become the seriesId *)

type [<Measure>] confirmedSeriesId
type ConfirmedSeriesId = int<confirmedSeriesId>
module ConfirmedSeriesId =
    let wellKnownId = 0<confirmedSeriesId>
    let toString (value : ConfirmedSeriesId) : string = string %value

type [<Measure>] confirmedEpochId
type ConfirmedEpochId = int<confirmedEpochId>
module ConfirmedEpochId =
    let unknown = -1<confirmedEpochId>
    let initial = 0<confirmedEpochId>
    let value (value : ConfirmedEpochId) : int = %value
    let next (value : ConfirmedEpochId) : ConfirmedEpochId = % (%value + 1)
    let toString (value : ConfirmedEpochId) : string = string %value
