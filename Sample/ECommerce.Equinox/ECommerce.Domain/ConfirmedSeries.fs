/// Maintains a pointer into the Epoch chain for a given Series
/// Allows the Ingester to determine the current Epoch into which it commence writing via ReadIngestionEpochId
/// As an Epoch is marked `Closed`, the Ingester will mark a new Epoch `Started` on this aggregate via MarkIngestionEpochId
module ECommerce.Domain.ConfirmedSeries

let [<Literal>] CategoryName = "ConfirmedSeries"
// TOCONSIDER: if you need multiple lists series/epochs in a single system, the Series and Epoch streams should have a SeriesId in the stream name
// See also the implementation in the feedSource template, where the Series aggregate also functions as an index of series held in the system
let streamId () = FsCodec.StreamId.gen ConfirmedSeriesId.toString ConfirmedSeriesId.wellKnownId

// NB - these types and the union case names reflect the actual storage formats and hence need to be versioned with care
[<RequireQualifiedAccess>]
module Events =

    type Event =
        | Started of            {| epochId : ConfirmedEpochId |}
        | Snapshotted of        {| active : ConfirmedEpochId |}
        interface TypeShape.UnionContract.IUnionContract
    let codec = Store.Codec.gen<Event>
    let codecJsonElement = Store.Codec.genJsonElement<Event>

module Fold =

    type State = ConfirmedEpochId option
    let initial = None
    let private evolve _state = function
        | Events.Started e -> Some e.epochId
        | Events.Snapshotted e -> Some e.active
    let fold : State -> Events.Event seq -> State = Seq.fold evolve

    let isOrigin = function Events.Snapshotted _ -> true | _ -> false
    let toSnapshot s = Events.Snapshotted {| active = Option.get s |}

let interpret epochId (state : Fold.State) = [|
    if state |> Option.forall (fun cur -> cur < epochId) && epochId >= ConfirmedEpochId.initial then
        yield Events.Started {| epochId = epochId |}|]

type Service internal (resolve : unit -> Equinox.Decider<Events.Event, Fold.State>) =

    /// Determines the current active epoch
    /// Uses cached values as epoch transitions are rare, and caller needs to deal with the inherent race condition in any case
    member _.ReadIngestionEpochId() : Async<ConfirmedEpochId> =
        let decider = resolve ()
        decider.Query(Option.defaultValue ConfirmedEpochId.initial)

    /// Mark specified `epochId` as live for the purposes of ingesting
    /// Writers are expected to react to having writes to an epoch denied (due to it being Closed) by anointing a successor via this
    member _.MarkIngestionEpochId epochId : Async<unit> =
        let decider = resolve ()
        decider.Transact(interpret epochId, load = Equinox.LoadOption.AnyCachedValue)

module Config =

    let private (|Category|) = function
        | Store.Config.Memory store ->            Store.Memory.create CategoryName Events.codec Fold.initial Fold.fold store
        | Store.Config.Cosmos (context, cache) -> Store.Cosmos.createSnapshotted CategoryName Events.codecJsonElement Fold.initial Fold.fold (Fold.isOrigin, Fold.toSnapshot) (context, cache)
        | Store.Config.Dynamo (context, cache) -> Store.Dynamo.createSnapshotted CategoryName Events.codec Fold.initial Fold.fold (Fold.isOrigin, Fold.toSnapshot) (context, cache)
        | Store.Config.Esdb (context, cache) ->   Store.Esdb.createUnoptimized CategoryName Events.codec Fold.initial Fold.fold (context, cache)
        | Store.Config.Sss (context, cache) ->    Store.Sss.createUnoptimized CategoryName Events.codec Fold.initial Fold.fold (context, cache)
    let create (Category cat) = Service(streamId >> Store.createDecider cat)
