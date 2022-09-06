module ECommerce.Reactor.Program

open ECommerce.Infrastructure
open ECommerce.Reactor.Infrastructure // SourceConfig etc
open Serilog
open System
open System.Threading.Tasks

module Config = ECommerce.Domain.Config

type Configuration(tryGet) =
    inherit Args.Configuration(tryGet)
    member _.DynamoIndexTable =             tryGet Args.INDEX_TABLE

let [<Literal>] AppName = "ECommerce.Reactor"

module Args =

    open Argu

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-p"; Unique>]   PrometheusPort of int
        | [<AltCommandLine "-g"; Mandatory>] ConsumerGroupName of string
        | [<AltCommandLine "-r"; Unique>]   MaxReadAhead of int
        | [<AltCommandLine "-w"; Unique>]   MaxWriters of int
        | [<AltCommandLine "-s"; Unique>]   StateIntervalM of float
        | [<AltCommandLine "-i"; Unique>]   IdleDelayMs of int
        | [<AltCommandLine "-W"; Unique>]   WakeForResults
        | [<CliPrefix(CliPrefix.None); Last>] Cosmos of ParseResults<CosmosSourceParameters>
        | [<CliPrefix(CliPrefix.None); Last>] Dynamo of ParseResults<DynamoSourceParameters>
        // | [<CliPrefix(CliPrefix.None); Last>] Esdb of ParseResults<Args.EsdbParameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "request Verbose Logging. Default: off."
                | PrometheusPort _ ->       "port from which to expose a Prometheus /metrics endpoint. Default: off (optional if environment variable PROMETHEUS_PORT specified)"
                | ConsumerGroupName _ ->    "Projector consumer group name."
                | MaxReadAhead _ ->         "maximum number of batches to let processing get ahead of completion. Default: 16."
                | MaxWriters _ ->           "maximum number of concurrent streams on which to process at any time. Default: 8."
                | StateIntervalM _ ->       "Interval at which to report Propulsion Statistics. Default: 10"
                | IdleDelayMs _ ->          "Idle delay for scheduler. Default 1000ms"
                | WakeForResults _ ->       "Wake for all results to provide optimal throughput"
                | Cosmos _ ->               "specify CosmosDB input parameters."
                | Dynamo _ ->               "specify DynamoDB input parameters."
                // | Esdb _ ->                 "specify EventStoreDB input parameters."
    and Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let maxReadAhead =                  a.GetResult(MaxReadAhead, 2)
        let maxConcurrentProcessors =       a.GetResult(MaxWriters, 8)
        let consumerGroupName =             a.GetResult ConsumerGroupName
        member _.ProcessorParams() =        Log.Information("Reacting... {consumerGroupName}, reading {maxReadAhead} ahead, {dop} writers",
                                                            consumerGroupName, maxReadAhead, maxConcurrentProcessors)
                                            (consumerGroupName, maxReadAhead, maxConcurrentProcessors)
        member val Verbose =                a.Contains Parameters.Verbose
        member val PrometheusPort =         a.TryGetResult PrometheusPort |> Option.orElseWith (fun () -> c.PrometheusPort)
        member val StatsInterval =          TimeSpan.FromMinutes 1.
        member val StateInterval =          a.GetResult(StateIntervalM, 10.) |> TimeSpan.FromMinutes
        member val PurgeInterval =          TimeSpan.FromHours 1.
        member val IdleDelay =              a.GetResult(IdleDelayMs, 1000) |> TimeSpan.FromMilliseconds
        member val WakeForResults =         a.Contains WakeForResults
        member val Store : Choice<CosmosSourceArguments, DynamoSourceArguments> =
                                            match a.GetSubCommand() with
                                            | Cosmos a -> Choice1Of2 <| CosmosSourceArguments(c, a)
                                            | Dynamo a -> Choice2Of2 <| DynamoSourceArguments(c, a)
                                            | a -> Args.missingArg $"Unexpected Store subcommand %A{a}"
        member x.StoreVerbose =             match x.Store with Choice1Of2 s -> s.Verbose | Choice2Of2 s -> s.Verbose
        member x.ConnectStoresAndMonitored(cache) : Config.Store<_> * (ILogger -> string -> SourceConfig) * (ILogger -> unit) * Config.Store<_> =
            match x.Store with
            | Choice1Of2 cosmos ->
                let client, monitored = cosmos.ConnectStoreAndMonitored()
                let buildSourceConfig log groupName =
                    let leases, startFromTail, maxItems, lagFrequency = cosmos.MonitoringParams(log)
                    let checkpointConfig = CosmosCheckpointConfig.Persistent (groupName, startFromTail, maxItems, lagFrequency)
                    SourceConfig.Cosmos (monitored, leases, checkpointConfig)
                let context = client |> CosmosStoreContext.create
                let store = Config.Store.Cosmos (context, cache)
                store, buildSourceConfig, Equinox.CosmosStore.Core.Log.InternalMetrics.dump, store
            | Choice2Of2 dynamo ->
                let context = dynamo.Connect()
                let buildSourceConfig log groupName =
                    let indexStore, startFromTail, batchSizeCutoff, tailSleepInterval, streamsDop = dynamo.MonitoringParams(log)
                    let checkpoints = dynamo.CreateCheckpointStore(groupName, cache)
                    let load = DynamoLoadModeConfig.Hydrate (context, streamsDop)
                    SourceConfig.Dynamo (indexStore, checkpoints, load, startFromTail, batchSizeCutoff, tailSleepInterval, x.StatsInterval)
                let store = Config.Store.Dynamo (context, cache)
                store, buildSourceConfig, Equinox.DynamoStore.Core.Log.InternalMetrics.dump, store
(*
            | Choice3Of3 dynamo ->
                let conn = a.Connect(log, AppName, EventStore.Client.NodePreference.Leader)
                let context = conn |> EventStoreContext.create
                let store = Config.Store.Esdb (context, cache)
                let checkpoints = args.CreateCheckpointStore(store)
                let handle = Reactor.Config.create (store, store)
                let sink = buildSink args log handle
                let source =
                    let maxItems, tailSleepInterval = a.MonitoringParams(log)
                    let includeBodies = true
                    Propulsion.EventStoreDb.EventStoreSource(
                        log, args.StatsInterval,
                        conn.ReadConnection, maxItems, tailSleepInterval,
                        checkpoints, sink, includeBodies
                    ).Start()
*)
        // member x.DumpStoreMetrics =         SourceArgs.dumpMetrics x.SourceStore
(*
        member val CheckpointInterval =     TimeSpan.FromHours 1.
        member private x.CheckpointStoreConfig(mainStore : Config.Store<_>) : Args.CheckpointStore.Config =
            match mainStore with
            | ECommerce.Domain.Config.Store.Cosmos (context, cache) -> Args.CheckpointStore.Config.Cosmos (context, cache)
            | ECommerce.Domain.Config.Store.Dynamo (context, cache) -> Args.CheckpointStore.Config.Dynamo (context, cache)
            | ECommerce.Domain.Config.Store.Memory _ ->                failwith "Unexpected"
            | ECommerce.Domain.Config.Store.Esdb (_, cache) ->
                match x.SourceStore with
                | SourceArgs.Store.Esdb a ->
                    match a.TargetStoreArgs with
                    | TargetStoreArguments.Cosmos a ->
                        let context = a.Connect() |> Async.RunSynchronously |> CosmosStoreContext.create
                        Args.CheckpointStore.Config.Cosmos (context, cache)
                    | TargetStoreArguments.Dynamo a ->
                        let context = a.Connect() |> DynamoStoreContext.create
                        Args.CheckpointStore.Config.Dynamo (context, cache)
                 | _ -> failwith "unexpected"
        member x.CreateCheckpointStore(mainStore) : Propulsion.Feed.IFeedCheckpointStore =
            let config = x.CheckpointStoreConfig(mainStore)
            Args.CheckpointStore.create (x.Group, x.CheckpointInterval) ECommerce.Domain.Config.log config
*)
    and [<NoEquality; NoComparison>] CosmosSourceParameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-m">]           ConnectionMode of Microsoft.Azure.Cosmos.ConnectionMode
        | [<AltCommandLine "-s">]           Connection of string
        | [<AltCommandLine "-d">]           Database of string
        | [<AltCommandLine "-c">]           Container of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesWaitTime of float

        | [<AltCommandLine "-a"; Unique>]   LeaseContainer of string
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-mi"; Unique>]  MaxItems of int
        | [<AltCommandLine "-l"; Unique>]   LagFreqM of float
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "request Verbose Logging from ChangeFeedProcessor and Store. Default: off"
                | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                | Database _ ->             "specify a database name for store. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                | Container _ ->            "specify a container name for store. (optional if environment variable EQUINOX_COSMOS_CONTAINER specified)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 5."
                | Retries _ ->              "specify operation retries. Default: 9."
                | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds. Default: 30."

                | LeaseContainer _ ->       "specify Container Name (in this [target] Database) for Leases container. Default: `SourceContainer` + `-aux`."
                | FromTail _ ->             "(iff the Consumer Name is fresh) - force skip to present Position. Default: Never skip an event."
                | MaxItems _ ->             "maximum item count to supply for the Change Feed query. Default: use response size limit"
                | LagFreqM _ ->             "specify frequency (minutes) to dump lag stats. Default: 1"
    and CosmosSourceArguments(c : Args.Configuration, a : ParseResults<CosmosSourceParameters>) =
        let discovery =                     a.TryGetResult CosmosSourceParameters.Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
        let mode =                          a.TryGetResult ConnectionMode
        let timeout =                       a.GetResult(Timeout, 5.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(CosmosSourceParameters.Retries, 9)
        let maxRetryWaitTime =              a.GetResult(RetriesWaitTime, 30.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
        let database =                      a.TryGetResult Database  |> Option.defaultWith (fun () -> c.CosmosDatabase)
        let containerId =                   a.TryGetResult Container |> Option.defaultWith (fun () -> c.CosmosContainer)
        let leaseContainerId =              a.GetResult(LeaseContainer, containerId + "-aux")
        let fromTail =                      a.Contains CosmosSourceParameters.FromTail
        let maxItems =                      a.TryGetResult CosmosSourceParameters.MaxItems
        let lagFrequency =                  a.GetResult(LagFreqM, 1.) |> TimeSpan.FromMinutes
        member _.Verbose =                  a.Contains CosmosSourceParameters.Verbose
        member private _.ConnectLeases() =  connector.CreateUninitialized(database, leaseContainerId)
        member x.MonitoringParams(log : ILogger) =
            let leases : Microsoft.Azure.Cosmos.Container = x.ConnectLeases()
            log.Information("ChangeFeed Leases Database {db} Container {container}. MaxItems limited to {maxItems}",
                leases.Database.Id, leases.Id, Option.toNullable maxItems)
            if fromTail then log.Warning("(If new projector group) Skipping projection of all existing events.")
            (leases, fromTail, maxItems, lagFrequency)
        member x.ConnectStoreAndMonitored() = connector.ConnectStoreAndMonitored(database, containerId)
    and [<NoEquality; NoComparison>] DynamoSourceParameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-s">]           ServiceUrl of string
        | [<AltCommandLine "-sa">]          AccessKey of string
        | [<AltCommandLine "-ss">]          SecretKey of string
        | [<AltCommandLine "-t">]           Table of string
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesTimeoutS of float
        | [<AltCommandLine "-i">]           IndexTable of string
        | [<AltCommandLine "-is">]          IndexSuffix of string
        | [<AltCommandLine "-mi"; Unique>]  MaxItems of int
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-d">]           StreamsDop of int
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | ServiceUrl _ ->           "specify a server endpoint for a Dynamo account. (optional if environment variable " + SERVICE_URL + " specified)"
                | AccessKey _ ->            "specify an access key id for a Dynamo account. (optional if environment variable " + ACCESS_KEY + " specified)"
                | SecretKey _ ->            "specify a secret access key for a Dynamo account. (optional if environment variable " + SECRET_KEY + " specified)"
                | Retries _ ->              "specify operation retries (default: 9)."
                | RetriesTimeoutS _ ->      "specify max wait-time including retries in seconds (default: 60)"
                | Table _ ->                "specify a table name for the primary store. (optional if environment variable " + TABLE + " specified)"
                | IndexTable _ ->           "specify a table name for the index store. (optional if environment variable " + INDEX_TABLE + " specified. default: `Table`+`IndexSuffix`)"
                | IndexSuffix _ ->          "specify a suffix for the index store. (optional if environment variable " + INDEX_TABLE + " specified. default: \"-index\")"
                | MaxItems _ ->             "maximum events to load in a batch. Default: 100"
                | FromTail _ ->             "(iff the Consumer Name is fresh) - force skip to present Position. Default: Never skip an event."
                | StreamsDop _ ->           "parallelism when loading events from Store Feed Source. Default 4"
    and DynamoSourceArguments(c : Configuration, a : ParseResults<DynamoSourceParameters>) =
        let serviceUrl =                    a.TryGetResult ServiceUrl |> Option.defaultWith (fun () -> c.DynamoServiceUrl)
        let accessKey =                     a.TryGetResult AccessKey  |> Option.defaultWith (fun () -> c.DynamoAccessKey)
        let secretKey =                     a.TryGetResult SecretKey  |> Option.defaultWith (fun () -> c.DynamoSecretKey)
        let table =                         a.TryGetResult Table      |> Option.defaultWith (fun () -> c.DynamoTable)
        let indexSuffix =                   a.GetResult(IndexSuffix, "-index")
        let indexTable =                    a.TryGetResult IndexTable |> Option.orElseWith  (fun () -> c.DynamoIndexTable) |> Option.defaultWith (fun () -> table + indexSuffix)
        let fromTail =                      a.Contains FromTail
        let tailSleepInterval =             TimeSpan.FromMilliseconds 500.
        let maxItems =                      a.GetResult(MaxItems, 100)
        let streamsDop =                    a.GetResult(StreamsDop, 4)
        let timeout =                       a.GetResult(RetriesTimeoutS, 60.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(Retries, 9)
        let connector =                     Equinox.DynamoStore.DynamoStoreConnector(serviceUrl, accessKey, secretKey, timeout, retries)
        let client =                        connector.CreateClient()
        let indexStoreClient =              lazy client.ConnectStore("Index", indexTable)
        member val Verbose =                a.Contains Verbose
        member _.Connect() =                connector.LogConfiguration()
                                            client.ConnectStore("Main", table) |> DynamoStoreContext.create
        member _.MonitoringParams(log : ILogger) =
            log.Information("DynamoStoreSource MaxItems {maxItems} Hydrater parallelism {streamsDop}", maxItems, streamsDop)
            let indexStoreClient = indexStoreClient.Value
            if fromTail then log.Warning("(If new projector group) Skipping projection of all existing events.")
            indexStoreClient, fromTail, maxItems, tailSleepInterval, streamsDop
        member _.CreateCheckpointStore(group, cache) =
            let indexTable = indexStoreClient.Value
            indexTable.CreateCheckpointService(group, cache, Config.log)
(*
    and [<NoEquality; NoComparison>] EsdbSourceParameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-c">]           Connection of string
        | [<AltCommandLine "-p"; Unique>]   Credentials of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int

// Not implemented in Propulsion.EventStoreDb yet (#good-first-issue)
//        | [<AltCommandLine "-Z"; Unique>]   FromTail

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<Args.CosmosParameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Dynamo of ParseResults<Args.DynamoParameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | Connection _ ->           "EventStore Connection String. (optional if environment variable EQUINOX_ES_CONNECTION specified)"
                | Credentials _ ->          "Credentials string for EventStore (used as part of connection string, but NOT logged). Default: use EQUINOX_ES_CREDENTIALS environment variable (or assume no credentials)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."

//                | FromTail ->               "Start the processing from the Tail"

                | Cosmos _ ->               "CosmosDB Target Store parameters (also used for checkpoint storage)."
                | Dynamo _ ->               "DynamoDB Target Store parameters (also used for checkpoint storage)."

    [<RequireQualifiedAccess; NoComparison; NoEquality>]
    type TargetStoreArguments = Cosmos of Args.Cosmos.Arguments | Dynamo of Args.Dynamo.Arguments

    type EsdbSourceArguments(c : Configuration, a : ParseResults<Parameters>) =
        let        tailSleepInterval =      TimeSpan.FromSeconds 0.5
        let        maxItems =               100
        member val Verbose =                a.Contains Verbose
        member val ConnectionString =       a.TryGetResult(Connection) |> Option.defaultWith (fun () -> c.EventStoreConnection)
        member val Credentials =            a.TryGetResult(Credentials) |> Option.orElseWith (fun () -> c.EventStoreCredentials) |> Option.toObj
        member val Retries =                a.GetResult(Retries, 3)
        member val Timeout =                a.GetResult(Timeout, 20.) |> TimeSpan.FromSeconds

        member x.Connect(log: ILogger, appName, nodePreference) =
            let connection = x.ConnectionString
            log.Information("EventStore {discovery}", connection)
            let discovery = String.Join(";", connection, x.Credentials) |> Equinox.EventStoreDb.Discovery.ConnectionString
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Equinox.EventStoreDb.EventStoreConnector(x.Timeout, x.Retries, tags=tags)
                .Establish(appName, discovery, Equinox.EventStoreDb.ConnectionStrategy.ClusterSingle nodePreference)
        member _.MonitoringParams(log : ILogger) =
            log.Information("EventStoreSource MaxItems {maxItems}", maxItems)
            maxItems, tailSleepInterval

        (*  Propulsion.EventStoreDb does not implement a checkpoint storage mechanism,
            perhaps port https://github.com/absolutejam/Propulsion.EventStoreDB ?
            or fork/finish https://github.com/jet/dotnet-templates/pull/81
            alternately one could use a SQL Server DB via Propulsion.SqlStreamStore *)
        // NOTE as a result we borrow the target/read model store to host the checkpoints in the case of the Reactor app
        member val TargetStoreArgs : TargetStoreArguments =
            match a.GetSubCommand() with
            | Cosmos cosmos -> TargetStoreArguments.Cosmos (Args.Cosmos.Arguments (c, cosmos))
            | Dynamo dynamo -> TargetStoreArguments.Dynamo (Args.Dynamo.Arguments (c, dynamo))
            | _ -> Args.missingArg "Must specify `cosmos` or `dynamo` target store when source is `esdb`"
*)

    /// Parse the commandline; can throw exceptions in response to missing arguments and/or `-h`/`--help` args
    let parse tryGetConfigValue argv : Arguments =
        let programName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name
        let parser = ArgumentParser.Create<Parameters>(programName=programName)
        Arguments(Configuration tryGetConfigValue, parser.ParseCommandLine argv)

open Propulsion.Internal // AwaitKeyboardInterruptAsTaskCanceledException

let build (args : Args.Arguments) =
    let consumerGroupName, maxReadAhead, maxConcurrentStreams = args.ProcessorParams()
    let cache = Equinox.Cache (AppName, sizeMb = 10)
    let store, buildSourceConfig, dumpMetrics, targetStore = args.ConnectStoresAndMonitored cache
    let buildProjector dump dop groupName filter handle =
        let log = Log.forGroup groupName // needs to have a `group` tag for Propulsion.Streams Prometheus metrics
        let sink =
            let stats = Reactor.Stats(log, args.StatsInterval, args.StateInterval, args.StoreVerbose, ?logExternalStats = dump)
            Reactor.Config.StartSink(log, stats, handle, maxReadAhead, dop,
                                     wakeForResults = args.WakeForResults, idleDelay = args.IdleDelay, purgeInterval = args.PurgeInterval)
        let source, _awaitSource =
            let sourceConfig = buildSourceConfig log groupName
            Reactor.Config.StartSource(log, sink, sourceConfig, filter)
        sink, source
    let filter, handle = Reactor.Config.create (store, targetStore)
    buildProjector (Some dumpMetrics) maxConcurrentStreams consumerGroupName filter handle

let run (args : Args.Arguments) = async {
    use _ = args.PrometheusPort |> Option.map startMetricsServer |> Option.toObj
    let sink, source = build args
    return! [|  Async.AwaitKeyboardInterruptAsTaskCanceledException()
                source.AwaitWithStopOnCancellation()
                sink.AwaitWithStopOnCancellation()
            |] |> Async.Parallel |> Async.Ignore<unit array> }

[<EntryPoint>]
let main argv =
    try let args = Args.parse EnvVar.tryGet argv
        let metrics = Sinks.tags AppName |> Sinks.equinoxAndPropulsionReactorMetrics
        try Log.Logger <- LoggerConfiguration().Configure(verbose=args.Verbose).Sinks(metrics, args.StoreVerbose).CreateLogger()
            try run args |> Async.RunSynchronously; 0
            with e when not (e :? Args.MissingArg) && not (e :? TaskCanceledException) -> Log.Fatal(e, "Exiting"); 2
        finally Log.CloseAndFlush()
    with Args.MissingArg msg -> eprintfn "%s" msg; 1
        | :? Argu.ArguParseException as e -> eprintfn "%s" e.Message; 1
        | e -> eprintfn "Exception %s" e.Message; 1
