/// Commandline arguments and/or secrets loading specifications
module ECommerce.Reactor.Infrastructure

open System

[<RequireQualifiedAccess; NoEquality; NoComparison>]
type SourceConfig =
    | Memory of
        store : Equinox.MemoryStore.VolatileStore<struct (int * ReadOnlyMemory<byte>)>
    | Cosmos of
        monitoredContainer : Microsoft.Azure.Cosmos.Container
        * leasesContainer : Microsoft.Azure.Cosmos.Container
        * checkpoints : CosmosCheckpointConfig
    | Dynamo of
        indexStore : Equinox.DynamoStore.DynamoStoreClient
        * checkpoints : Propulsion.Feed.IFeedCheckpointStore
        * loading : DynamoLoadModeConfig
        * startFromTail : bool
        * batchSizeCutoff : int
        * tailSleepInterval : TimeSpan
        * statsInterval : TimeSpan
and CosmosCheckpointConfig =
    | Ephemeral of processorName : string
    | Persistent of processorName : string * startFromTail : bool * maxItems : int option * lagFrequency : TimeSpan
and [<NoEquality; NoComparison>]
    DynamoLoadModeConfig =
    | Hydrate of monitoredContext : Equinox.DynamoStore.DynamoStoreContext * hydrationConcurrency : int

module SourceConfig =
    module Memory =
        open Propulsion.MemoryStore
        let start log (sink : Propulsion.Streams.Default.Sink) streamFilter
            (store : Equinox.MemoryStore.VolatileStore<_>) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let source = MemoryStoreSource(log, store, streamFilter, sink)
            source.Start(), Some (fun _propagationDelay -> source.Monitor.AwaitCompletion(ignoreSubsequent = false))
    module Cosmos =
        open Propulsion.CosmosStore
        let start log (sink : Propulsion.Streams.Default.Sink) streamFilter
            (monitoredContainer, leasesContainer, checkpointConfig) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let parseFeedDoc = EquinoxSystemTextJsonParser.enumStreamEvents streamFilter
            let observer = CosmosStoreSource.CreateObserver(log, sink.StartIngester, Seq.collect parseFeedDoc)
            let source =
                match checkpointConfig with
                | Ephemeral processorName ->
                    let withStartTime1sAgo (x : Microsoft.Azure.Cosmos.ChangeFeedProcessorBuilder) =
                        x.WithStartTime(let t = DateTime.UtcNow in t.AddSeconds -1.)
                    let lagFrequency = TimeSpan.FromMinutes 1.
                    CosmosStoreSource.Start(log, monitoredContainer, leasesContainer, processorName, observer, startFromTail = false,
                                            customize = withStartTime1sAgo, lagReportFreq = lagFrequency)
                | Persistent (processorName, startFromTail, maxItems, lagFrequency) ->
                    CosmosStoreSource.Start(log, monitoredContainer, leasesContainer, processorName, observer, startFromTail,
                                            ?maxItems = maxItems, lagReportFreq = lagFrequency)
            source, None
    module Dynamo =
        open Propulsion.DynamoStore
        let start (log, storeLog) (sink : Propulsion.Streams.Default.Sink) streamFilter
            (indexStore, checkpoints, loadModeConfig, startFromTail, tailSleepInterval, batchSizeCutoff, statsInterval) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let loadMode =
                match loadModeConfig with
                | Hydrate (monitoredContext, hydrationConcurrency) -> LoadMode.Hydrated (streamFilter, hydrationConcurrency, monitoredContext)
            let source =
                DynamoStoreSource(
                    log, statsInterval,
                    indexStore, batchSizeCutoff, tailSleepInterval,
                    checkpoints, sink, loadMode, fromTail = startFromTail, storeLog = storeLog,
                    trancheIds = [|Propulsion.Feed.TrancheId.parse "0"|]) // TEMP filter for additional clones of index data in target Table
            source.Start(), Some (fun propagationDelay -> source.Monitor.AwaitCompletion(propagationDelay, ignoreSubsequent = false))

    let start (log, storeLog) sink streamFilter : SourceConfig -> Propulsion.Pipeline * (TimeSpan -> Async<unit>) option = function
        | SourceConfig.Memory volatileStore ->
            Memory.start log sink streamFilter volatileStore
        | SourceConfig.Cosmos (monitored, leases, checkpointConfig) ->
            Cosmos.start log sink streamFilter (monitored, leases, checkpointConfig)
        | SourceConfig.Dynamo (indexStore, checkpoints, loading, startFromTail, batchSizeCutoff, tailSleepInterval, statsInterval) ->
            Dynamo.start (log, storeLog) sink streamFilter (indexStore, checkpoints, loading, startFromTail, tailSleepInterval, batchSizeCutoff, statsInterval)
