module ECommerce.Reactor.Program

open ECommerce.Infrastructure
open Propulsion.EventStore
open Serilog
open System

exception MissingArg of message : string with override this.Message = this.message

type Configuration(tryGet) =

    let get key =
        match tryGet key with
        | Some value -> value
        | None -> raise (MissingArg (sprintf "Missing Argument/Environment Variable %s" key))

    member _.CosmosConnection =             get "EQUINOX_COSMOS_CONNECTION"
    member _.CosmosDatabase =               get "EQUINOX_COSMOS_DATABASE"
    member _.CosmosContainer =              get "EQUINOX_COSMOS_CONTAINER"
    member _.EventStorePort =               tryGet "EQUINOX_ES_PORT" |> Option.map int
    member _.EventStoreProjectionPort =     tryGet "EQUINOX_ES_PROJ_PORT" |> Option.map int
    member _.EventStoreHost =               get "EQUINOX_ES_HOST"
    member _.EventStoreProjectionHost =     tryGet "EQUINOX_ES_PROJ_HOST"
    member _.EventStoreUsername =           get "EQUINOX_ES_USERNAME"
    member _.EventStoreProjectionUsername = tryGet "EQUINOX_ES_PROJ_USERNAME"
    member _.EventStorePassword =           get "EQUINOX_ES_PASSWORD"
    member _.EventStoreProjectionPassword = tryGet "EQUINOX_ES_PROJ_PASSWORD"

module Args =

    open Argu
    open Equinox.EventStore
    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-g"; Mandatory>] ProcessorName of string
        | [<AltCommandLine "-r"; Unique>]   MaxReadAhead of int
        | [<AltCommandLine "-w"; Unique>]   MaxWriters of int
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<CosmosSourceParameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Es of ParseResults<EsSourceParameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "request Verbose Logging. Default: off."
                | ProcessorName _ ->        "Projector consumer group name."
                | MaxReadAhead _ ->         "maximum number of batches to let processing get ahead of completion. Default: 16."
                | MaxWriters _ ->           "maximum number of concurrent streams on which to process at any time. Default: 8."
                | Cosmos _ ->               "specify CosmosDB input parameters."
                | Es _ ->                   "specify EventStore input parameters."
    and Arguments(c : Configuration, a : ParseResults<Parameters>) =
        member val Verbose =                a.Contains Parameters.Verbose
        member val ProcessorName =          a.GetResult ProcessorName
        member val MaxReadAhead =           a.GetResult(MaxReadAhead, 16)
        member val MaxConcurrentStreams =   a.GetResult(MaxWriters, 8)
        member val StatsInterval =          TimeSpan.FromMinutes 1.
        member val StateInterval =          TimeSpan.FromMinutes 5.
        member val Source : Choice<EsSourceArguments, CosmosSourceArguments> =
            match a.TryGetSubCommand() with
            | Some (Es es) -> Choice1Of2 (EsSourceArguments (c, es))
            | Some (Parameters.Cosmos cosmos) -> Choice2Of2 (CosmosSourceArguments (c, cosmos))
            | _ -> raise (MissingArg "Must specify one of cosmos or es for Src")
        member x.SourceParams() : Choice<EsSourceArguments*Equinox.CosmosStore.CosmosStoreContext*ReaderSpec, _> =
            match x.Source with
            | Choice1Of2 srcE ->
                let startPos, cosmos = srcE.StartPos, srcE.Cosmos
                Log.Information("Processing Consumer Group {groupName} from {startPos} (force: {forceRestart}) in Database {db} Container {container}",
                    x.ProcessorName, startPos, srcE.ForceRestart, cosmos.DatabaseId, cosmos.ContainerId)
                Log.Information("Ingesting in batches of [{minBatchSize}..{batchSize}], reading up to {maxReadAhead} uncommitted batches ahead",
                    srcE.MinBatchSize, srcE.StartingBatchSize, x.MaxReadAhead)
                let context = cosmos.Connect() |> Async.RunSynchronously |> CosmosStoreContext.create
                Choice1Of2 (srcE, context,
                    {   groupName = x.ProcessorName; start = startPos; checkpointInterval = srcE.CheckpointInterval; tailInterval = srcE.TailInterval
                        forceRestart = srcE.ForceRestart
                        batchSize = srcE.StartingBatchSize; minBatchSize = srcE.MinBatchSize; gorge = srcE.Gorge; streamReaders = 0 })
            | Choice2Of2 srcC ->
                let leases = srcC.ConnectLeases()
                Log.Information("Reacting... {dop} writers, max {maxReadAhead} batches read ahead", x.MaxConcurrentStreams, x.MaxReadAhead)
                Log.Information("ChangeFeed {processorName} Leases Database {db} Container {container}. MaxItems limited to {maxItems}",
                    x.ProcessorName, srcC.DatabaseId, srcC.ContainerId, Option.toNullable srcC.MaxItems)
                if srcC.FromTail then Log.Warning("(If new projector group) Skipping projection of all existing events.")
                Log.Information("ChangeFeed Lag stats interval {lagS:n0}s", let f = srcC.LagFrequency in f.TotalSeconds)
                let storeClient, monitored = srcC.ConnectStoreAndMonitored()
                let context = CosmosStoreContext.create storeClient
                Choice2Of2 (context, monitored, leases, x.ProcessorName, srcC.FromTail, srcC.MaxItems, srcC.LagFrequency)
    and [<NoEquality; NoComparison>] CosmosSourceParameters =
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-mi"; Unique>]  MaxItems of int
        | [<AltCommandLine "-l"; Unique>]   LagFreqM of float
        | [<AltCommandLine "-a"; Unique>]   LeaseContainer of string

        | [<AltCommandLine "-m">]           ConnectionMode of Microsoft.Azure.Cosmos.ConnectionMode
        | [<AltCommandLine "-s">]           Connection of string
        | [<AltCommandLine "-d">]           Database of string
        | [<AltCommandLine "-c"; Unique>]   Container of string // Actually Mandatory, but stating that is not supported
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesWaitTime of float

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<CosmosParameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | FromTail ->               "(iff the Consumer Name is fresh) - force skip to present Position. Default: Never skip an event."
                | MaxItems _ ->             "maximum item count to request from feed. Default: unlimited"
                | LagFreqM _ ->             "frequency (in minutes) to dump lag stats. Default: 1"
                | LeaseContainer _ ->       "specify Container Name for Leases container. Default: `sourceContainer` + `-aux`."

                | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                | Database _ ->             "specify a database name for Cosmos account. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                | Container _ ->            "specify a container name within `Database`"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 5."
                | Retries _ ->              "specify operation retries. Default: 1."
                | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds. Default: 5."
                | Cosmos _ ->               "CosmosDb Sink parameters."
    and CosmosSourceArguments(c : Configuration, a : ParseResults<CosmosSourceParameters>) =
        let discovery =                     a.TryGetResult CosmosSourceParameters.Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
        let mode =                          a.TryGetResult CosmosSourceParameters.ConnectionMode
        let timeout =                       a.GetResult(CosmosSourceParameters.Timeout, 5.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(CosmosSourceParameters.Retries, 1)
        let maxRetryWaitTime =              a.GetResult(CosmosSourceParameters.RetriesWaitTime, 5.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
        member val DatabaseId =             a.TryGetResult CosmosSourceParameters.Database |> Option.defaultWith (fun () -> c.CosmosDatabase)
        member val ContainerId =            a.GetResult CosmosSourceParameters.Container

        member val FromTail =               a.Contains CosmosSourceParameters.FromTail
        member val MaxItems =               a.TryGetResult MaxItems
        member val LagFrequency : TimeSpan = a.GetResult(LagFreqM, 1.) |> TimeSpan.FromMinutes
        member val private LeaseContainerId = a.TryGetResult CosmosSourceParameters.LeaseContainer
        member private x.ConnectLeases containerId = connector.CreateUninitialized(x.DatabaseId, containerId)
        member x.ConnectLeases() =          match x.LeaseContainerId with
                                            | None ->    x.ConnectLeases(x.ContainerId + "-aux")
                                            | Some sc -> x.ConnectLeases(sc)
        member x.ConnectStoreAndMonitored() = connector.ConnectStoreAndMonitored(x.DatabaseId, x.ContainerId)
        member val Cosmos =
            match a.TryGetSubCommand() with
            | Some (CosmosSourceParameters.Cosmos cosmos) -> CosmosArguments (c, cosmos)
            | _ -> raise (MissingArg "Must specify cosmos details")
    and [<NoEquality; NoComparison>] EsSourceParameters =
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-g"; Unique>]   Gorge of int
        | [<AltCommandLine "-t"; Unique>]   Tail of intervalS: float
        | [<AltCommandLine "--force"; Unique>] ForceRestart
        | [<AltCommandLine "-m"; Unique>]   BatchSize of int

        | [<AltCommandLine "-mim"; Unique>] MinBatchSize of int
        | [<AltCommandLine "-pos"; Unique>] Position of int64
        | [<AltCommandLine "-c"; Unique>]   Chunk of int
        | [<AltCommandLine "-pct"; Unique>] Percent of float

        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-oh">]          HeartbeatTimeout of float
        | [<AltCommandLine "-h">]           Host of string
        | [<AltCommandLine "-x">]           Port of int
        | [<AltCommandLine "-u">]           Username of string
        | [<AltCommandLine "-p">]           Password of string
        | [<AltCommandLine "-hp">]          ProjHost of string
        | [<AltCommandLine "-xp">]          ProjPort of int
        | [<AltCommandLine "-up">]          ProjUsername of string
        | [<AltCommandLine "-pp">]          ProjPassword of string

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<CosmosParameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | FromTail ->               "Start the processing from the Tail"
                | Gorge _ ->                "Request Parallel readers phase during initial catchup, running one chunk (256MB) apart. Default: off"
                | Tail _ ->                 "attempt to read from tail at specified interval in Seconds. Default: 1"
                | ForceRestart _ ->         "Forget the current committed position; start from (and commit) specified position. Default: start from specified position or resume from committed."
                | BatchSize _ ->            "maximum item count to request from feed. Default: 4096"
                | MinBatchSize _ ->         "minimum item count to drop down to in reaction to read failures. Default: 512"
                | Position _ ->             "EventStore $all Stream Position to commence from"
                | Chunk _ ->                "EventStore $all Chunk to commence from"
                | Percent _ ->              "EventStore $all Stream Position to commence from (as a percentage of current tail position)"

                | Verbose ->                "Include low level Store logging."
                | Host _ ->                 "TCP mode: specify EventStore hostname to connect to directly. Clustered mode: use Gossip protocol against all A records returned from DNS query. (optional if environment variable EQUINOX_ES_HOST specified)"
                | Port _ ->                 "specify EventStore custom port. Uses value of environment variable EQUINOX_ES_PORT if specified. Defaults for Cluster and Direct TCP/IP mode are 30778 and 1113 respectively."
                | Username _ ->             "specify username for EventStore. (optional if environment variable EQUINOX_ES_USERNAME specified)"
                | Password _ ->             "specify Password for EventStore. (optional if environment variable EQUINOX_ES_PASSWORD specified)"
                | ProjHost _ ->             "TCP mode: specify Projection EventStore hostname to connect to directly. Clustered mode: use Gossip protocol against all A records returned from DNS query. Defaults to value of es host (-h) unless environment variable EQUINOX_ES_PROJ_HOST is specified."
                | ProjPort _ ->             "specify Projection EventStore custom port. Defaults to value of es port (-x) unless environment variable EQUINOX_ES_PROJ_PORT is specified."
                | ProjUsername _ ->         "specify username for Projection EventStore. Defaults to value of es user (-u) unless environment variable EQUINOX_ES_PROJ_USERNAME is specified."
                | ProjPassword _ ->         "specify Password for Projection EventStore. Defaults to value of es password (-p) unless environment variable EQUINOX_ES_PROJ_PASSWORD is specified."
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."
                | HeartbeatTimeout _ ->     "specify heartbeat timeout in seconds. Default: 1.5."

                | Cosmos _ ->               "CosmosDB (Checkpoint) Store parameters."
    and EsSourceArguments(c : Configuration, a : ParseResults<EsSourceParameters>) =
        let ts (x : TimeSpan) = x.TotalSeconds
        let discovery (host, port) =
            match port with
            | None ->   Discovery.GossipDns            host
            | Some p -> Discovery.GossipDnsCustomPort (host, p)
        let host =                          a.TryGetResult Host |> Option.defaultWith (fun () -> c.EventStoreHost)
        let port =                          a.TryGetResult Port |> Option.orElseWith (fun () -> c.EventStorePort)
        let user =                          a.TryGetResult Username |> Option.defaultWith (fun () -> c.EventStoreUsername)
        let password =                      a.TryGetResult Password |> Option.defaultWith (fun () -> c.EventStorePassword)
        member val Gorge =                  a.TryGetResult Gorge
        member val TailInterval =           a.GetResult(Tail, 1.) |> TimeSpan.FromSeconds
        member val ForceRestart =           a.Contains ForceRestart
        member val StartingBatchSize =      a.GetResult(BatchSize, 4096)
        member val MinBatchSize =           a.GetResult(MinBatchSize, 512)
        member val StartPos =
            match a.TryGetResult Position, a.TryGetResult Chunk, a.TryGetResult Percent, a.Contains EsSourceParameters.FromTail with
            | Some p, _, _, _ ->            Absolute p
            | _, Some c, _, _ ->            StartPos.Chunk c
            | _, _, Some p, _ ->            Percentage p
            | None, None, None, true ->     StartPos.TailOrCheckpoint
            | None, None, None, _ ->        StartPos.StartOrCheckpoint
        member val Host =                   host
        member val Port =                   port
        member val User =                   user
        member val Password =               password
        member val ProjPort =               match a.TryGetResult ProjPort with
                                            | Some x -> Some x
                                            | None -> c.EventStoreProjectionPort |> Option.orElse port
        member val ProjHost =               match a.TryGetResult ProjHost with
                                            | Some x -> x
                                            | None -> c.EventStoreProjectionHost |> Option.defaultValue host
        member val ProjUser =               match a.TryGetResult ProjUsername with
                                            | Some x -> x
                                            | None -> c.EventStoreProjectionUsername |> Option.defaultValue user
        member val ProjPassword =           match a.TryGetResult ProjPassword with
                                            | Some x -> x
                                            | None -> c.EventStoreProjectionPassword |> Option.defaultValue password
        member val Retries =                a.GetResult(EsSourceParameters.Retries, 3)
        member val Timeout =                a.GetResult(EsSourceParameters.Timeout, 20.) |> TimeSpan.FromSeconds
        member val Heartbeat =              a.GetResult(HeartbeatTimeout, 1.5) |> TimeSpan.FromSeconds

        member x.ConnectProj(log: ILogger, storeLog: ILogger, appName, nodePreference) =
            let discovery = discovery (x.ProjHost, x.ProjPort)
            log.ForContext("projHost", x.ProjHost).ForContext("projPort", x.ProjPort)
                .Information("Projection EventStore {discovery} heartbeat: {heartbeat}s Timeout: {timeout}s Retries {retries}",
                    discovery, ts x.Heartbeat, ts x.Timeout, x.Retries)
            let log=if storeLog.IsEnabled Serilog.Events.LogEventLevel.Debug then Logger.SerilogVerbose storeLog else Logger.SerilogNormal storeLog
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Connector(x.ProjUser, x.ProjPassword, x.Timeout, x.Retries, log=log, heartbeatTimeout=x.Heartbeat, tags=tags)
                .Connect(appName + "-Proj", discovery, nodePreference) |> Async.RunSynchronously

        member x.Connect(log: ILogger, storeLog: ILogger, appName, connectionStrategy) =
            let discovery = discovery (x.Host, x.Port)
            log.ForContext("host", x.Host).ForContext("port", x.Port)
                .Information("EventStore {discovery} heartbeat: {heartbeat}s Timeout: {timeout}s Retries {retries}",
                    discovery, ts x.Heartbeat, ts x.Timeout, x.Retries)
            let log=if storeLog.IsEnabled Serilog.Events.LogEventLevel.Debug then Logger.SerilogVerbose storeLog else Logger.SerilogNormal storeLog
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Connector(x.User, x.Password, x.Timeout, x.Retries, log=log, heartbeatTimeout=x.Heartbeat, tags=tags)
                .Establish(appName, discovery, connectionStrategy) |> Async.RunSynchronously

        member val CheckpointInterval =     TimeSpan.FromHours 1.
        member val Cosmos : CosmosArguments =
            match a.TryGetSubCommand() with
            | Some (EsSourceParameters.Cosmos cosmos) -> CosmosArguments (c, cosmos)
            | _ -> raise (MissingArg "Must specify `cosmos` checkpoint store when source is `es`")
    and [<NoEquality; NoComparison>] CosmosParameters =
        | [<AltCommandLine "-s">]           Connection of string
        | [<AltCommandLine "-m">]           ConnectionMode of Microsoft.Azure.Cosmos.ConnectionMode
        | [<AltCommandLine "-d">]           Database of string
        | [<AltCommandLine "-c">]           Container of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesWaitTime of float
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                | Database _ ->             "specify a database name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                | Container _ ->            "specify a container name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_CONTAINER specified)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 5."
                | Retries _ ->              "specify operation retries. Default: 1."
                | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds. Default: 5."
    and CosmosArguments(c : Configuration, a : ParseResults<CosmosParameters>) =
        let discovery =                     a.TryGetResult Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
        let mode =                          a.TryGetResult ConnectionMode
        let timeout =                       a.GetResult(Timeout, 30.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(Retries, 9)
        let maxRetryWaitTime =              a.GetResult(RetriesWaitTime, 30.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode=mode)
        member val DatabaseId =             a.TryGetResult Database |> Option.defaultWith (fun () -> c.CosmosDatabase)
        member val ContainerId =            a.TryGetResult Container |> Option.defaultWith (fun () -> c.CosmosContainer)
        member x.Connect() =                connector.ConnectStore("Main", x.DatabaseId, x.ContainerId)

    /// Parse the commandline; can throw exceptions in response to missing arguments and/or `-h`/`--help` args
    let parse tryGetConfigValue argv : Arguments =
        let programName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name
        let parser = ArgumentParser.Create<Parameters>(programName=programName)
        Arguments(Configuration tryGetConfigValue, parser.ParseCommandLine argv)

let [<Literal>] AppName = "ReactorTemplate"

module Checkpoints =

    open Equinox.CosmosStore

    // In this implementation, we keep the checkpoints in Cosmos when consuming from EventStore
    module Cosmos =

        let codec = FsCodec.NewtonsoftJson.Codec.Create<Checkpoint.Events.Event>()
        let access = AccessStrategy.Custom (Checkpoint.Fold.isOrigin, Checkpoint.Fold.transmute)
        let create groupName (context, cache) =
            let caching = CachingStrategy.SlidingWindow (cache, TimeSpan.FromMinutes 20.)
            let cat = CosmosStoreCategory(context, codec, Checkpoint.Fold.fold, Checkpoint.Fold.initial, caching, access)
            let resolve streamName = cat.Resolve(streamName, Equinox.AllowStale)
            Checkpoint.CheckpointSeries(groupName, resolve)

module EventStoreContext =

    let create connection =
        Equinox.EventStore.EventStoreContext(connection, Equinox.EventStore.BatchingPolicy(maxBatchSize=500))

open Propulsion.CosmosStore.Infrastructure // AwaitKeyboardInterruptAsTaskCancelledException
open ECommerce.Domain

let build (args : Args.Arguments) =
    match args.SourceParams() with
    | Choice1Of2 (srcE, context, spec) ->
        let connectEs () = srcE.Connect(Log.Logger, Log.Logger, AppName, Equinox.EventStore.ConnectionStrategy.ClusterSingle Equinox.EventStore.NodePreference.Master)
        let connectProjEs () = srcE.ConnectProj(Log.Logger, Log.Logger, AppName, Equinox.EventStore.NodePreference.PreferSlave)

        let cache = Equinox.Cache(AppName, sizeMb = 10)
        let checkpoints = Checkpoints.Cosmos.create spec.groupName (context, cache)
        let esStore =
            let esConn = connectEs ()
            Config.Store.Esdb (EventStoreContext.create esConn, cache)
        let cosmosStore = Config.Store.Cosmos (context, cache)
        let handle = Reactor.Config.create (esStore, cosmosStore)
        let stats = Reactor.Stats(Log.Logger, args.StatsInterval, args.StateInterval)
        let sink = Propulsion.Streams.StreamsProjector.Start(Log.Logger, args.MaxReadAhead, args.MaxConcurrentStreams, handle, stats, args.StatsInterval)
        let runPipeline =
            let tryMapEvent (x : EventStore.ClientAPI.ResolvedEvent) =
                match x.Event with
                | e when not e.IsJson || e.EventStreamId.StartsWith "$" -> None
                | PropulsionStreamEvent e -> Some e
            EventStoreSource.Run(
                Log.Logger, sink, checkpoints, connectProjEs, spec, tryMapEvent,
                args.MaxReadAhead, args.StatsInterval)
        [ runPipeline; sink.AwaitWithStopOnCancellation() ]
    | Choice2Of2 (context, monitored, leases, processorName, startFromTail, maxItems, lagFrequency) ->
        let cache = Equinox.Cache(AppName, sizeMb = 10)
        let store = Config.Store.Cosmos (context, cache)
        let handle = Reactor.Config.create (store, store)
        let stats = Reactor.Stats(Log.Logger, args.StatsInterval, args.StateInterval)
        let sink = Propulsion.Streams.StreamsProjector.Start(Log.Logger, args.MaxReadAhead, args.MaxConcurrentStreams, handle, stats, args.StatsInterval)
        let source =
            let mapToStreamItems = Seq.collect Propulsion.CosmosStore.EquinoxNewtonsoftParser.enumStreamEvents
            let observer = Propulsion.CosmosStore.CosmosStoreSource.CreateObserver(Log.Logger, sink.StartIngester, mapToStreamItems)
            Propulsion.CosmosStore.CosmosStoreSource.Start(Log.Logger, monitored, leases, processorName, observer, startFromTail, ?maxItems=maxItems, lagReportFreq=lagFrequency)
        [ Async.AwaitKeyboardInterruptAsTaskCancelledException(); source.AwaitWithStopOnCancellation(); sink.AwaitWithStopOnCancellation() ]

let run args =
    Async.Parallel (build args) |> Async.Ignore<unit array>

[<EntryPoint>]
let main argv =
    try let args = Args.parse EnvVar.tryGet argv
        try Log.Logger <- LoggerConfiguration().Configure(verbose=args.Verbose).CreateLogger()
            try run args |> Async.RunSynchronously; 0
            with e when not (e :? MissingArg) -> Log.Fatal(e, "Exiting"); 2
        finally Log.CloseAndFlush()
    with MissingArg msg -> eprintfn "%s" msg; 1
        | :? Argu.ArguParseException as e -> eprintfn "%s" e.Message; 1
        | e -> eprintf "Exception %s" e.Message; 1
