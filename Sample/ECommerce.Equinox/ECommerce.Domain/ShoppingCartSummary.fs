module ECommerce.Domain.ShoppingCartSummary

let [<Literal>] CategoryName = "ShoppingCartSummary"

let streamId = FsCodec.StreamId.gen CartId.toString

module Events =

    type Ingested = { version : int64; value : Summary }
     and Summary = { items : Item[]; status : Status; clientId : ClientId }
     and Item = { productId : ProductId; unitPrice : decimal; quantity : int }
     and Status = Pending | Confirmed
    type Event =
        | Ingested of Ingested
        interface TypeShape.UnionContract.IUnionContract
    let codec = Store.Codec.gen<Event>
    let codecJsonElement = Store.Codec.genJsonElement<Event>

module Fold =

    type State = Events.Ingested option
    let initial = None
    let private evolve _s = function
        | Events.Ingested e ->   Some e
    let fold = Seq.fold evolve
    let toSnapshot (s : State) = s.Value |> Events.Ingested

module Details =

    type View = { (* id *) clientId : ClientId; status : Events.Status; items : Item[] }
     and Item = { productId : ProductId; unitPrice : decimal; quantity : int }

    let render : Fold.State -> View option = function
        | None -> None
        | Some { value = v } ->
            let items = [| for { productId = productId; quantity = q; unitPrice = p } in v.items ->
                             { productId = productId; unitPrice = p; quantity = q } |]
            Some { clientId = v.clientId; status = v.status; items = items }

module Ingest =

    let summarizeShoppingCartView (view : ShoppingCart.Details.View) : Events.Summary =
        let mapStatus = function
            | ShoppingCart.Fold.Pending -> Events.Pending
            | ShoppingCart.Fold.Confirmed -> Events.Confirmed
        {   clientId = view.clientId; status = mapStatus view.status
            items = [| for i in view.items -> { productId = i.productId; unitPrice = i.unitPrice; quantity = i.quantity } |] }

    let decide (version : int64, value : Events.Summary): Fold.State -> bool * Events.Event[] = function
        | Some { version = v } when v >= version -> false, [||]
        | None -> false, [||]
        | _ -> true, [| Events.Ingested { version = version; value = value } |]

type Service internal (resolve : CartId -> Equinox.Decider<Events.Event, Fold.State>) =

    member _.Read(cartId) : Async<Details.View option> =
        let decider = resolve cartId
        decider.Query(Details.render)

    member _.TryIngest(cartId, version, value) : Async<bool> =
        let decider = resolve cartId
        decider.Transact(Ingest.decide (version, Ingest.summarizeShoppingCartView value))

module Config =

    let private (|Category|) = function
        | Store.Config.Memory store ->            Store.Memory.create CategoryName Events.codec Fold.initial Fold.fold store
        | Store.Config.Cosmos (context, cache) -> Store.Cosmos.createRollingState CategoryName Events.codecJsonElement Fold.initial Fold.fold Fold.toSnapshot (context, cache)
        | Store.Config.Dynamo (context, cache) -> Store.Dynamo.createRollingState CategoryName Events.codec Fold.initial Fold.fold Fold.toSnapshot (context, cache)
        | Store.Config.Esdb _ | Store.Config.Sss _ -> failwith "Not implemented: For EventStore/Sss its suggested to do a cached read from the write side"
    let create (Category cat) = Service(streamId >> Store.createDecider cat)
