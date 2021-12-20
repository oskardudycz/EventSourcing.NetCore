/// Application Service that controls the ingestion of Items into a chain of `Epoch` streams
/// - the `Series` aggregate maintains a pointer to the current Epoch
/// - as `Epoch`s complete (have `Closed` events logged), we update the `active` Epoch in the Series to reference the new one
/// The fact that each request walks forward from a given start point until it either gets to append (or encounters a prior insertion)
///   means we can guarantee the insertion/deduplication to be idempotent and insert exactly once per completed execution
module ECommerce.Domain.ConfirmedIngester

open FSharp.UMX // %

/// Ensures any given item is only added to the list exactly once by virtue of the following protocol:
/// 1. Caller obtains an origin epoch via ActiveIngestionEpochId, storing that alongside the source item
/// 2. Caller deterministically obtains that origin epoch to supply to Ingest/TryIngest such that retries can be idempotent
type Service internal (log : Serilog.ILogger, epochs : ConfirmedEpoch.Service, series : ConfirmedSeries.Service, linger) =

    let uninitializedSentinel : int = %ConfirmedEpochId.unknown
    let mutable currentEpochId_ = uninitializedSentinel
    let currentEpochId () = if currentEpochId_ <> uninitializedSentinel then Some %currentEpochId_ else None

    let tryIngest (items : (ConfirmedEpochId * ConfirmedEpoch.Events.Cart)[][]) = async {
        let rec aux ingestedItems items = async {
            let epochId = items |> Array.minBy fst |> fst
            let epochItems, futureEpochItems = items |> Array.partition (fun (e,_) -> e = epochId)
            let! res = epochs.Ingest(epochId, Array.map snd epochItems)
            let ingestedItemIds = Array.append ingestedItems res.accepted
            let logLevel =
                if res.residual.Length <> 0 || futureEpochItems.Length <> 0 || Array.isEmpty res.accepted then Serilog.Events.LogEventLevel.Information
                else Serilog.Events.LogEventLevel.Debug
            log.Write(logLevel, "Added {count}/{total} items to {epochId} Residual {residual} Future {future}",
                      res.accepted.Length, epochItems.Length, epochId, res.residual.Length, futureEpochItems.Length)
            let nextEpochId = ConfirmedEpochId.next epochId
            let pushedToNextEpoch = res.residual |> Array.map (fun x -> nextEpochId, x)
            match Array.append pushedToNextEpoch futureEpochItems with
            | [||] ->
                // Any writer noticing we've moved to a new Epoch shares the burden of marking it active in the Series
                let newActiveEpochId = if res.closed then nextEpochId else epochId
                if currentEpochId_ < %newActiveEpochId then
                    log.Information("Marking {epochId} active", newActiveEpochId)
                    do! series.MarkIngestionEpochId(newActiveEpochId)
                    System.Threading.Interlocked.CompareExchange(&currentEpochId_, %newActiveEpochId, currentEpochId_) |> ignore
                return ingestedItemIds
            | remaining -> return! aux ingestedItemIds remaining }
        return! aux [||] (Array.concat items)
    }

    /// In the overall processing using an Ingester, we frequently have a Scheduler running N streams concurrently
    /// If each thread works in isolation, they'll conflict with each other as they feed the Items into the batch in epochs.Ingest
    /// Instead, we enable concurrent requests to coalesce by having requests converge in this AsyncBatchingGate
    /// This has the following critical effects:
    /// - Traffic to CosmosDB is naturally constrained to a single flight in progress
    ///   (BatchingGate does not release next batch for execution until current has succeeded or throws)
    /// - RU consumption for writing to the batch is optimized (1 write inserting 1 event document vs N writers writing N)
    /// - Peak throughput is more consistent as latency is not impacted by the combination of having to:
    ///   a) back-off, re-read and retry if there's a concurrent write Optimistic Concurrency Check failure when writing the stream
    ///   b) enter a prolonged period of retries if multiple concurrent writes trigger rate limiting and 429s from CosmosDB
    ///   c) readers will less frequently encounter sustained 429s on the batch
    let batchedIngest = Equinox.Core.AsyncBatchingGate(tryIngest, linger)

    /// Slot the items into the series of epochs.
    /// Returns the subset that actually got fed in this time around, exclusive of items that did not trigger events per idempotency rules.
    member _.IngestMany(originEpoch, items) : Async<CartId seq> = async {
        if Array.isEmpty items then return Seq.empty else

        let! results = batchedIngest.Execute [| for x in items -> originEpoch, x |]
        return System.Linq.Enumerable.Intersect(Seq.map ConfirmedEpoch.itemId items, results)
    }

    /// Slot the item into the series of epochs.
    /// Returns true if the item actually got included into an Epoch this time (i.e. will be false if it was an idempotent retry).
    member _.TryIngest(originEpoch, cart) : Async<bool> = async {
        let! result = batchedIngest.Execute [| originEpoch, cart |]
        return result |> Seq.contains (ConfirmedEpoch.itemId cart)
    }

    /// Exposes the current high water mark epoch - i.e. the tip epoch to which appends are presently being applied.
    /// The fact that any Ingest call for a given item (or set of items) always commences from the same origin is key to exactly once insertion guarantee.
    /// Caller should first store this alongside the item in order to deterministically be able to start from the same origin in idempotent retry cases.
    /// Uses cached values as epoch transitions are rare, and caller needs to deal with the inherent race condition in any case
    member _.ActiveIngestionEpochId() : Async<ConfirmedEpochId> =
        match currentEpochId () with
        | Some currentEpochId -> async { return currentEpochId }
        | None -> series.ReadIngestionEpochId()

module Config =

    let create_ linger maxItemsPerEpoch store =
        let shouldClose candidateItems currentItems = Array.length currentItems + Array.length candidateItems >= maxItemsPerEpoch
        let epochs = ConfirmedEpoch.Config.create shouldClose store
        let series = ConfirmedSeries.Config.create store
        let log = Serilog.Log.ForContext<Service>()
        Service(log, epochs, series, linger = linger)
    let create =
        let defaultLinger = System.TimeSpan.FromMilliseconds 200.
        let maxItemsPerEpoch = 10_000
        create_ defaultLinger maxItemsPerEpoch
