/// Generic Service that controls the deterministic application of requests that insert items into a chain of `Epoch` streams
/// - the `Series` aggregate maintains a pointer to the current Epoch
/// - as `Epoch`s complete (have `Closed` events logged), we update the `active` Epoch in the Series to reference the new one
/// The fact that each request walks forward from a given start point until it either gets to append (or encounters a prior insertion)
///   means we can guarantee the insertion/deduplication to be idempotent and insert exactly once per completed execution
module ECommerce.Domain.ExactlyOnceIngester

open FSharp.UMX // %

type IngestResult<'req, 'res> = { accepted : 'res[]; closed : bool; residual : 'req[] }

module Internal =

    let unknown<[<Measure>]'m> = UMX.tag -1
    let next<[<Measure>]'m> (value : int<'m>) = UMX.tag<'m>(UMX.untag value + 1)

/// Ensures any given item is only added to the series exactly once by virtue of the following protocol:
/// 1. Caller obtains an origin epoch via ActiveIngestionEpochId, storing that alongside the source item
/// 2. Caller deterministically obtains that origin epoch to supply to Ingest/TryIngest such that retries can be idempotent
type Service<[<Measure>]'id, 'req, 'res, 'outcome> internal
    (   log : Serilog.ILogger,
        readActiveEpoch : unit -> Async<int<'id>>,
        markActiveEpoch : int<'id> -> Async<unit>,
        ingest : int<'id> * 'req [] -> Async<IngestResult<'req, 'res>>,
        mapResults : 'res [] -> 'outcome seq,
        linger) =

    let uninitializedSentinel : int = %Internal.unknown
    let mutable currentEpochId_ = uninitializedSentinel
    let currentEpochId () = if currentEpochId_ <> uninitializedSentinel then Some %currentEpochId_ else None

    let tryIngest (reqs : (int<'id> * 'req)[][]) =
        let rec aux ingestedItems items = async {
            let epochId = items |> Array.minBy fst |> fst
            let epochItems, futureEpochItems = items |> Array.partition (fun (e, _ : 'req) -> e = epochId)
            let! res = ingest (epochId, Array.map snd epochItems)
            let ingestedItemIds = Array.append ingestedItems res.accepted
            let logLevel =
                if res.residual.Length <> 0 || futureEpochItems.Length <> 0 || Array.isEmpty res.accepted then Serilog.Events.LogEventLevel.Information
                else Serilog.Events.LogEventLevel.Debug
            log.Write(logLevel, "Added {count}/{total} items to {epochId} Residual {residual} Future {future}",
                      res.accepted.Length, epochItems.Length, epochId, res.residual.Length, futureEpochItems.Length)
            let nextEpochId = Internal.next epochId
            let pushedToNextEpoch = res.residual |> Array.map (fun x -> nextEpochId, x)
            match Array.append pushedToNextEpoch futureEpochItems with
            | [||] ->
                // Any writer noticing we've moved to a new Epoch shares the burden of marking it active in the Series
                let newActiveEpochId = if res.closed then nextEpochId else epochId
                if currentEpochId_ < %newActiveEpochId then
                    log.Information("Marking {epochId} active", newActiveEpochId)
                    do! markActiveEpoch newActiveEpochId
                    System.Threading.Interlocked.CompareExchange(&currentEpochId_, %newActiveEpochId, currentEpochId_) |> ignore
                return ingestedItemIds
            | remaining -> return! aux ingestedItemIds remaining }
        aux [||] (Array.concat reqs)

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
    let batchedIngest = Equinox.Core.Batching.Batcher(tryIngest, Linger = linger)

    /// Run the requests over a chain of epochs.
    /// Returns the subset that actually got handled this time around (exclusive of items that did not trigger events per idempotency rules).
    member _.IngestMany(originEpoch, reqs) : Async<'outcome seq> = async {
        if Array.isEmpty reqs then return Seq.empty else

        let! results = batchedIngest.Execute [| for x in reqs -> originEpoch, x |]
        return results |> mapResults
    }

    /// Exposes the current high water mark epoch - i.e. the tip epoch to which appends are presently being applied.
    /// The fact that any Ingest call for a given item (or set of items) always commences from the same origin is key to exactly once insertion guarantee.
    /// Caller should first store this alongside the item in order to deterministically be able to start from the same origin in idempotent retry cases.
    /// Uses cached values as epoch transitions are rare, and caller needs to deal with the inherent race condition in any case
    member _.ActiveIngestionEpochId() : Async<int<'id>> =
        match currentEpochId () with
        | Some currentEpochId -> async { return currentEpochId }
        | None -> readActiveEpoch()

let create log linger (readIngestionEpoch, markIngestionEpoch) (apply, mapResult) =
    Service<'id, 'req, 'res, 'outcome>(log, readIngestionEpoch, markIngestionEpoch, apply, mapResult, linger = linger)
