/// Commandline arguments and/or secrets loading specifications for applications other than Reactors
module ECommerce.Infrastructure.Args

open Serilog
open System

exception MissingArg of message : string with override this.Message = this.message
let missingArg msg = raise (MissingArg msg)

let [<Literal>] SERVICE_URL =               "EQUINOX_DYNAMO_SERVICE_URL"
let [<Literal>] ACCESS_KEY =                "EQUINOX_DYNAMO_ACCESS_KEY_ID"
let [<Literal>] SECRET_KEY =                "EQUINOX_DYNAMO_SECRET_ACCESS_KEY"
let [<Literal>] TABLE =                     "EQUINOX_DYNAMO_TABLE"
let [<Literal>] INDEX_TABLE =               "EQUINOX_DYNAMO_TABLE_INDEX"

type Configuration(tryGet : string -> string option) =

    member val tryGet =                     tryGet
    member _.get key =                      match tryGet key with Some value -> value | None -> missingArg $"Missing Argument/Environment Variable %s{key}"

    member x.CosmosConnection =             x.get "EQUINOX_COSMOS_CONNECTION"
    member x.CosmosDatabase =               x.get "EQUINOX_COSMOS_DATABASE"
    member x.CosmosContainer =              x.get "EQUINOX_COSMOS_CONTAINER"

    member x.DynamoServiceUrl =             x.get SERVICE_URL
    member x.DynamoAccessKey =              x.get ACCESS_KEY
    member x.DynamoSecretKey =              x.get SECRET_KEY
    member x.DynamoTable =                  x.get TABLE

    member x.EventStoreConnection =         x.get "EQUINOX_ES_CONNECTION"
    member _.EventStoreCredentials =        tryGet "EQUINOX_ES_CREDENTIALS"

    member x.PrometheusPort =               tryGet "PROMETHEUS_PORT" |> Option.map int

open Argu

module Cosmos =

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-m">]           ConnectionMode of Microsoft.Azure.Cosmos.ConnectionMode
        | [<AltCommandLine "-s">]           Connection of string
        | [<AltCommandLine "-d">]           Database of string
        | [<AltCommandLine "-c">]           Container of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesWaitTime of float
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose _ ->              "request verbose logging."
                | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                | Database _ ->             "specify a database name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                | Container _ ->            "specify a container name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_CONTAINER specified)"
                | Timeout _ ->              "specify operation timeout in seconds (default: 5)."
                | Retries _ ->              "specify operation retries (default: 1)."
                | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds (default: 5)"

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let discovery =                    a.TryGetResult Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
        let mode =                          a.TryGetResult ConnectionMode
        let timeout =                       a.GetResult(Timeout, 5.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(Retries, 1)
        let maxRetryWaitTime =              a.GetResult(RetriesWaitTime, 5.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
        let database =                      a.TryGetResult Database |> Option.defaultWith (fun () -> c.CosmosDatabase)
        let container =                     a.TryGetResult Container |> Option.defaultWith (fun () -> c.CosmosContainer)
        member val Verbose =                a.Contains Verbose
        member _.Connect() =                connector.ConnectStore("Main", database, container)

module Dynamo =

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-s">]           ServiceUrl of string
        | [<AltCommandLine "-sa">]          AccessKey of string
        | [<AltCommandLine "-ss">]          SecretKey of string
        | [<AltCommandLine "-t">]           Table of string
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesTimeoutS of float
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | ServiceUrl _ ->           "specify a server endpoint for a Dynamo account. (optional if environment variable " + SERVICE_URL + " specified)"
                | AccessKey _ ->            "specify an access key id for a Dynamo account. (optional if environment variable " + ACCESS_KEY + " specified)"
                | SecretKey _ ->            "specify a secret access key for a Dynamo account. (optional if environment variable " + SECRET_KEY + " specified)"
                | Table _ ->                "specify a table name for the primary store. (optional if environment variable " + TABLE + " specified)"
                | Retries _ ->              "specify operation retries (default: 1)."
                | RetriesTimeoutS _ ->      "specify max wait-time including retries in seconds (default: 5)"

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let serviceUrl =                    a.TryGetResult ServiceUrl |> Option.defaultWith (fun () -> c.DynamoServiceUrl)
        let accessKey =                     a.TryGetResult AccessKey  |> Option.defaultWith (fun () -> c.DynamoAccessKey)
        let secretKey =                     a.TryGetResult SecretKey  |> Option.defaultWith (fun () -> c.DynamoSecretKey)
        let table =                         a.TryGetResult Table      |> Option.defaultWith (fun () -> c.DynamoTable)
        let retries =                       a.GetResult(Retries, 1)
        let timeout =                       a.GetResult(RetriesTimeoutS, 5.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.DynamoStore.DynamoStoreConnector(serviceUrl, accessKey, secretKey, timeout, retries)
        member val Verbose =                a.Contains Verbose
        member _.Connect() =                connector.ConnectStore(connector.CreateClient(), "Main", table)

module Esdb =

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-c">]           Connection of string
        | [<AltCommandLine "-p"; Unique>]   Credentials of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Dynamo of ParseResults<Dynamo.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | Connection _ ->           "EventStore Connection String. (optional if environment variable EQUINOX_ES_CONNECTION specified)"
                | Credentials _ ->          "Credentials string for EventStore (used as part of connection string, but NOT logged). Default: use EQUINOX_ES_CREDENTIALS environment variable (or assume no credentials)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."

                // Feed Consumer app needs somewhere to store checkpoints
                // Here we align with the structure of the commandline parameters for the Reactor app and also require a Dynamo or Cosmos instance to be specified
                | Cosmos _ ->               "CosmosDB (Checkpoint) Store parameters (Not applicable for Web app)."
                | Dynamo _ ->               "DynamoDB (Checkpoint) Store parameters (Not applicable to Web app)."

    [<RequireQualifiedAccess; NoComparison; NoEquality>]
    type CheckpointStoreArguments = Cosmos of Cosmos.Arguments | Dynamo of Dynamo.Arguments

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        member val ConnectionString =       a.TryGetResult(Connection) |> Option.defaultWith (fun () -> c.EventStoreConnection)
        member val Credentials =            a.TryGetResult(Credentials) |> Option.orElseWith (fun () -> c.EventStoreCredentials) |> Option.toObj
        member val Retries =                a.GetResult(Retries, 3)
        member val Timeout =                a.GetResult(Timeout, 20.) |> TimeSpan.FromSeconds
        member _.Verbose =                  a.Contains Verbose

        (*  Propulsion.EventStoreDb does not implement a checkpoint storage mechanism,
            perhaps port https://github.com/absolutejam/Propulsion.EventStoreDB ?
            or fork/finish https://github.com/jet/dotnet-templates/pull/81
            alternately one could use a SQL Server DB via Propulsion.SqlStreamStore *)
        // NOTE only applicable to the FeedConsumer app
        member _.CheckpointStoreArgs : CheckpointStoreArguments =
            match a.GetSubCommand() with
            | Cosmos cosmos -> CheckpointStoreArguments.Cosmos (Cosmos.Arguments (c, cosmos))
            | Dynamo dynamo -> CheckpointStoreArguments.Dynamo (Dynamo.Arguments (c, dynamo))
            | _ -> missingArg "Must specify `cosmos` or `dynamo` target store when source is `esdb`"

        member x.Connect(log : ILogger, appName, nodePreference) =
            let connection = x.ConnectionString
            log.Information("EventStore {discovery}", connection)
            let discovery = String.Join(";", connection, x.Credentials) |> Equinox.EventStoreDb.Discovery.ConnectionString
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Equinox.EventStoreDb.EventStoreConnector(x.Timeout, x.Retries, tags = tags)
                .Establish(appName, discovery, Equinox.EventStoreDb.ConnectionStrategy.ClusterSingle nodePreference)

[<RequireQualifiedAccess; NoComparison; NoEquality>]
type Store = Cosmos of Cosmos.Arguments | Dynamo of Dynamo.Arguments | Esdb of Esdb.Arguments
let verboseRequested = function
    | Store.Cosmos a -> a.Verbose
    | Store.Dynamo a -> a.Verbose
    | Store.Esdb a -> a.Verbose
let dumpMetrics = function
    | Store.Cosmos _ -> Equinox.CosmosStore.Core.Log.InternalMetrics.dump
    | Store.Dynamo _ -> Equinox.DynamoStore.Core.Log.InternalMetrics.dump
    | Store.Esdb _ -> Equinox.EventStoreDb.Log.InternalMetrics.dump

// Type used to represent where checkpoints (for either the FeedConsumer position, or for a Reactor's Event Store subscription position) will be stored
// In a typical app you don't have anything like this as you'll simply use your primary Event Store (see)
module CheckpointStore =

    [<RequireQualifiedAccess; NoComparison; NoEquality>]
    type Config =
        | Cosmos of Equinox.CosmosStore.CosmosStoreContext * Equinox.Core.ICache
        | Dynamo of Equinox.DynamoStore.DynamoStoreContext * Equinox.Core.ICache

    let create (consumerGroup, checkpointInterval) storeLog : Config -> Propulsion.Feed.IFeedCheckpointStore = function
        | Config.Cosmos (context, cache) ->
            Propulsion.Feed.ReaderCheckpoint.CosmosStore.create storeLog (consumerGroup, checkpointInterval) (context, cache)
        | Config.Dynamo (context, cache) ->
            Propulsion.Feed.ReaderCheckpoint.DynamoStore.create storeLog (consumerGroup, checkpointInterval) (context, cache)

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
