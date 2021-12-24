module ECommerce.Reactor.Reactor

open ECommerce.Domain

[<RequireQualifiedAccess>]
type Outcome =
    /// Handler processed the span, with counts of used vs unused known event types
    | Ok of used : int * unused : int
    /// Handler processed the span, but idempotency checks resulted in no writes being applied; includes count of decoded events
    | Skipped of count : int
    /// Handler determined the events were not relevant to its duties and performed no actions
    /// e.g. wrong category, events that dont imply a state change
    | NotApplicable of count : int

/// Gathers stats based on the outcome of each Span processed for emission, at intervals controlled by `StreamsConsumer`
type Stats(log, statsInterval, stateInterval) =
    inherit Propulsion.Streams.Stats<Outcome>(log, statsInterval, stateInterval)

    let mutable ok, skipped, na = 0, 0, 0

    override _.HandleOk res = res |> function
        | Outcome.Ok (used, unused) -> ok <- ok + used; skipped <- skipped + unused
        | Outcome.Skipped count -> skipped <- skipped + count
        | Outcome.NotApplicable count -> na <- na + count
    override _.HandleExn(log, exn) =
        log.Information(exn, "Unhandled")

    override _.DumpStats() =
        if ok <> 0 || skipped <> 0 || na <> 0 then
            log.Information(" used {ok} skipped {skipped} n/a {na}", ok, skipped, na)
            ok <- 0; skipped <- 0; na <- 0

let handle
        (cartSummary : ShoppingCartSummaryHandler.Service)
        (confirmedCarts : ConfirmedHandler.Service)
        (stream, span : Propulsion.Streams.StreamSpan<_>) = async {
    match stream, span with
    | ShoppingCart.Reactions.Parse (cartId, events) ->
        match events with
        | ShoppingCart.Reactions.Confirmed ->
            let! _done = confirmedCarts.TrySummarizeConfirmed(cartId) in ()
        | _ -> ()
        match events with
        | ShoppingCart.Reactions.StateChanged ->
            let! worked, version' = cartSummary.TryIngestSummary(cartId)
            return  Propulsion.Streams.SpanResult.OverrideWritePosition version',
                    (if worked then Outcome.Ok (1, span.events.Length - 1) else Outcome.Skipped span.events.Length)
        | _ -> return Propulsion.Streams.SpanResult.AllProcessed, Outcome.NotApplicable span.events.Length
    | _ -> return Propulsion.Streams.SpanResult.AllProcessed, Outcome.NotApplicable span.events.Length }

module Config =

    let create (sourceStore, cosmosStore) =
        let cartSummary = ShoppingCartSummaryHandler.Config.create (sourceStore, cosmosStore)
        let confirmedCarts = ConfirmedHandler.Config.create (sourceStore, cosmosStore)
        handle cartSummary confirmedCarts
