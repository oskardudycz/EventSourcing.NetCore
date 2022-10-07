namespace ECommerce.Infrastructure

open System

[<RequireQualifiedAccess; NoEquality; NoComparison>]
type SourceConfig =
    | Memory of store : Equinox.MemoryStore.VolatileStore<struct (int * ReadOnlyMemory<byte>)>
    | Cosmos of monitoredContainer : Microsoft.Azure.Cosmos.Container
        * leasesContainer : Microsoft.Azure.Cosmos.Container
        * checkpoints : CosmosFeedConfig
        * tailSleepInterval : TimeSpan
    | Dynamo of indexStore : Equinox.DynamoStore.DynamoStoreClient
        * checkpoints : Propulsion.Feed.IFeedCheckpointStore
        * loading : DynamoLoadModeConfig
        * startFromTail : bool
        * batchSizeCutoff : int
        * tailSleepInterval : TimeSpan
        * statsInterval : TimeSpan
    | Esdb of client : EventStore.Client.EventStoreClient
        * checkpoints : Propulsion.Feed.IFeedCheckpointStore
        * hydrateBodies : bool
        * startFromTail : bool
        * batchSize : int
        * tailSleepInterval : TimeSpan
        * statsInterval : TimeSpan
    | Sss of client : SqlStreamStore.IStreamStore
        * checkpoints : Propulsion.Feed.IFeedCheckpointStore
        * hydrateBodies : bool
        * startFromTail : bool
        * batchSize : int
        * tailSleepInterval : TimeSpan
        * statsInterval : TimeSpan
and [<NoEquality; NoComparison>] CosmosFeedConfig =
    | Ephemeral of processorName : string
    | Persistent of processorName : string * startFromTail : bool * maxItems : int option * lagFrequency : TimeSpan
and [<NoEquality; NoComparison>] DynamoLoadModeConfig =
    | Hydrate of monitoredContext : Equinox.DynamoStore.DynamoStoreContext * hydrationConcurrency : int

module SourceConfig =
    module Memory =
        open Propulsion.MemoryStore
        let start log (sink : Propulsion.Streams.Default.Sink) categoryFilter
            (store : Equinox.MemoryStore.VolatileStore<_>) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let source = MemoryStoreSource(log, store, categoryFilter, sink)
            source.Start(), Some (fun _propagationDelay -> source.Monitor.AwaitCompletion(ignoreSubsequent = false))
    module Cosmos =
        open Propulsion.CosmosStore
        let start log (sink : Propulsion.Streams.Default.Sink) categoryFilter
            (monitoredContainer, leasesContainer, checkpointConfig, tailSleepInterval) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let parseFeedDoc = EquinoxSystemTextJsonParser.enumStreamEvents categoryFilter
            let observer = CosmosStoreSource.CreateObserver(log, sink.StartIngester, Seq.collect parseFeedDoc)
            let source =
                match checkpointConfig with
                | Ephemeral processorName ->
                    let withStartTime1sAgo (x : Microsoft.Azure.Cosmos.ChangeFeedProcessorBuilder) =
                        x.WithStartTime(let t = DateTime.UtcNow in t.AddSeconds -1.)
                    let lagFrequency = TimeSpan.FromMinutes 1.
                    CosmosStoreSource.Start(log, monitoredContainer, leasesContainer, processorName, observer,
                                            startFromTail = true, customize = withStartTime1sAgo, tailSleepInterval = tailSleepInterval,
                                            lagReportFreq = lagFrequency)
                | Persistent (processorName, startFromTail, maxItems, lagFrequency) ->
                    CosmosStoreSource.Start(log, monitoredContainer, leasesContainer, processorName, observer,
                                            startFromTail = startFromTail, ?maxItems = maxItems, tailSleepInterval = tailSleepInterval,
                                            lagReportFreq = lagFrequency)
            source, None
    module Dynamo =
        open Propulsion.DynamoStore
        let start (log, storeLog) (sink : Propulsion.Streams.Default.Sink) categoryFilter
            (indexStore, checkpoints, loadModeConfig, startFromTail, tailSleepInterval, batchSizeCutoff, statsInterval) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let loadMode =
                match loadModeConfig with
                | Hydrate (monitoredContext, hydrationConcurrency) -> LoadMode.Hydrated (categoryFilter, hydrationConcurrency, monitoredContext)
            let source =
                DynamoStoreSource(
                    log, statsInterval,
                    indexStore, batchSizeCutoff, tailSleepInterval,
                    checkpoints, sink, loadMode,
                    startFromTail = startFromTail, storeLog = storeLog)
                    // trancheIds = [|Propulsion.Feed.TrancheId.parse "0"|]) // TEMP filter for additional clones of index data in target Table
            let source = source.Start()
            source, Some (fun propagationDelay -> source.Monitor.AwaitCompletion(propagationDelay, ignoreSubsequent = false))
    module Esdb =
        open Propulsion.EventStoreDb
        let start log (sink : Propulsion.Streams.Default.Sink) categoryFilter
            (client, checkpoints, hydrateBodies, startFromTail, batchSize, tailSleepInterval, statsInterval) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let source =
                EventStoreSource(
                    log, statsInterval,
                    client, batchSize, tailSleepInterval,
                    checkpoints, sink, categoryFilter, hydrateBodies = hydrateBodies, startFromTail = startFromTail)
            let source = source.Start()
            source, Some (fun propagationDelay -> source.Monitor.AwaitCompletion(propagationDelay, ignoreSubsequent = false))
    module Sss =
        open Propulsion.SqlStreamStore
        let start log (sink : Propulsion.Streams.Default.Sink) categoryFilter
            (client, checkpoints, hydrateBodies, startFromTail, batchSize, tailSleepInterval, statsInterval) : Propulsion.Pipeline * (TimeSpan -> Async<unit>) option =
            let source =
                SqlStreamStoreSource(
                    log, statsInterval,
                    client, batchSize, tailSleepInterval,
                    checkpoints, sink, categoryFilter, hydrateBodies = hydrateBodies, startFromTail = startFromTail)
            let source = source.Start()
            source, Some (fun propagationDelay -> source.Monitor.AwaitCompletion(propagationDelay, ignoreSubsequent = false))

    let start (log, storeLog) sink categoryFilter : SourceConfig -> Propulsion.Pipeline * (TimeSpan -> Async<unit>) option = function
        | SourceConfig.Memory volatileStore ->
            Memory.start log sink categoryFilter volatileStore
        | SourceConfig.Cosmos (monitored, leases, checkpointConfig, tailSleepInterval) ->
            Cosmos.start log sink categoryFilter (monitored, leases, checkpointConfig, tailSleepInterval)
        | SourceConfig.Dynamo (indexStore, checkpoints, loading, startFromTail, batchSizeCutoff, tailSleepInterval, statsInterval) ->
            Dynamo.start (log, storeLog) sink categoryFilter (indexStore, checkpoints, loading, startFromTail, tailSleepInterval, batchSizeCutoff, statsInterval)
        | SourceConfig.Esdb (client, checkpoints, hydrateBodies, startFromTail, batchSize, tailSleepInterval, statsInterval) ->
            Esdb.start log sink categoryFilter (client, checkpoints, hydrateBodies, startFromTail, batchSize, tailSleepInterval, statsInterval)
        | SourceConfig.Sss (client, checkpoints, hydrateBodies, startFromTail, batchSize, tailSleepInterval, statsInterval) ->
            Sss.start log sink categoryFilter (client, checkpoints, hydrateBodies, startFromTail, batchSize, tailSleepInterval, statsInterval)
