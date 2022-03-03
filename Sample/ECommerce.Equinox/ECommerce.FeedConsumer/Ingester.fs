module ECommerce.FeedConsumer.Ingester

open System
open FeedConsumerTemplate.Domain

type Outcome = { added : int; notReady : int; dups : int }

/// Gathers stats based on the outcome of each Span processed for periodic emission
type Stats(log, statsInterval, stateInterval) =
    inherit Propulsion.Streams.Stats<Outcome>(log, statsInterval, stateInterval)

    let mutable added, notReady, dups = 0, 0, 0

    override _.HandleOk outcome =
        added <- added + outcome.added
        notReady <- notReady + outcome.notReady
        dups <- dups + outcome.dups

    override _.HandleExn(log, exn) =
        log.Information(exn, "Unhandled")

    override _.DumpStats() =
        base.DumpStats()
        if added <> 0 || notReady <> 0 || dups <> 0 then
            log.Information(" Added {added} Not Yet Shipped {notReady} Duplicates {dups}", added, notReady, dups)
            added <- 0; notReady <- 0; dups <- 0

module PipelineEvent =

    type Item = { id : TicketId; payload : string }
    let ofIndexAndItem index (item : Item) =
        FsCodec.Core.TimelineEvent.Create(
            index,
            "eventType",
            null,
            context = item)
    let (|ItemsForFc|_|) = function
        | FsCodec.StreamName.CategoryAndIds (_,[|_ ; FcId.Parse fc|]), (s : Propulsion.Streams.StreamSpan<_>) ->
            Some (fc, s.events |> Seq.map (fun e -> Unchecked.unbox<Item> e.Context))
        | _ -> None

let handle maxDop (stream, span) = async {
    match stream, span with
    | PipelineEvent.ItemsForFc (fc, items) ->
        // Take chunks of max 1000 in order to make handler latency be less 'lumpy'
        // What makes sense in terms of a good chunking size will vary depending on the workload in question
        let ticketIds = seq { for x in items -> x.id } |> Seq.truncate 1000 |> Seq.toArray
        let maybeAccept = Seq.distinct ticketIds |> Seq.mapi (fun i _x -> async {
            do! Async.Sleep(TimeSpan.FromSeconds 1.)
            return if i % 3 = 1 then Some 42 else None
        })
        let! results = Async.Parallel(maybeAccept, maxDegreeOfParallelism=maxDop)
        let ready = results |> Array.choose id
        let maybeAdd = ready |> Seq.mapi (fun i _x -> async {
            do! Async.Sleep(TimeSpan.FromSeconds 1.)
            return if i % 2 = 1 then Some 42 else None
        })
        let! added = Async.Parallel(maybeAdd, maxDegreeOfParallelism=maxDop)
        let outcome = { added = Seq.length added; notReady = results.Length - ready.Length; dups = results.Length - ticketIds.Length }
        return Propulsion.Streams.SpanResult.PartiallyProcessed ticketIds.Length, outcome
    | x -> return failwithf "Unexpected stream %O" x
}
