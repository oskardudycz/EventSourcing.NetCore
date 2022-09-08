module ECommerce.Reactor.Program

open ECommerce.Infrastructure
open Serilog
open System
open System.Threading.Tasks

module Config = ECommerce.Domain.Config

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
        | [<CliPrefix(CliPrefix.None); Last>] Cosmos of ParseResults<SourceArgs.Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Last>] Dynamo of ParseResults<SourceArgs.Dynamo.Parameters>
        | [<CliPrefix(CliPrefix.None); Last>] Esdb of ParseResults<SourceArgs.Esdb.Parameters>
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
                | Esdb _ ->                 "specify EventStoreDB input parameters."

    type Arguments(c : SourceArgs.Configuration, a : ParseResults<Parameters>) =
        let maxReadAhead =                  a.GetResult(MaxReadAhead, 2)
        let maxConcurrentProcessors =       a.GetResult(MaxWriters, 8)
        let consumerGroupName =             a.GetResult ConsumerGroupName
        member _.ProcessorParams() =        Log.Information("Reacting... {consumerGroupName}, reading {maxReadAhead} ahead, {dop} writers",
                                                            consumerGroupName, maxReadAhead, maxConcurrentProcessors)
                                            (consumerGroupName, maxReadAhead, maxConcurrentProcessors)
        member val Verbose =                a.Contains Verbose
        member val PrometheusPort =         a.TryGetResult PrometheusPort |> Option.orElseWith (fun () -> c.PrometheusPort)
        member val CacheSizeMb =            10
        member val StatsInterval =          TimeSpan.FromMinutes 1.
        member val StateInterval =          a.GetResult(StateIntervalM, 10.) |> TimeSpan.FromMinutes
        member val PurgeInterval =          TimeSpan.FromHours 1.
        member val IdleDelay =              a.GetResult(IdleDelayMs, 1000) |> TimeSpan.FromMilliseconds
        member val WakeForResults =         a.Contains WakeForResults
        member val Store : Choice<SourceArgs.Cosmos.Arguments, SourceArgs.Dynamo.Arguments, SourceArgs.Esdb.Arguments> =
                                            match a.GetSubCommand() with
                                            | Cosmos a -> Choice1Of3 <| SourceArgs.Cosmos.Arguments(c, a)
                                            | Dynamo a -> Choice2Of3 <| SourceArgs.Dynamo.Arguments(c, a)
                                            | Esdb a ->   Choice3Of3 <| SourceArgs.Esdb.Arguments(c, a)
                                            | a -> Args.missingArg $"Unexpected Store subcommand %A{a}"
        member x.StoreVerbose =             match x.Store with
                                            | Choice1Of3 s -> s.Verbose
                                            | Choice2Of3 s -> s.Verbose
                                            | Choice3Of3 s -> s.Verbose
        member x.DumpStoreMetrics =         match x.Store with
                                            | Choice1Of3 _ -> Equinox.CosmosStore.Core.Log.InternalMetrics.dump
                                            | Choice2Of3 _ -> Equinox.DynamoStore.Core.Log.InternalMetrics.dump
                                            | Choice3Of3 _ -> Equinox.EventStoreDb.Log.InternalMetrics.dump

        member x.ConnectStoreSourceAndTarget() : Config.Store<_> * (ILogger -> string -> SourceConfig) * Config.Store<_> =
            let cache = Equinox.Cache (AppName, sizeMb = x.CacheSizeMb)
            match x.Store with
            | Choice1Of3 a ->
                let client, monitored = a.ConnectStoreAndMonitored()
                let buildSourceConfig log groupName =
                    let leases, startFromTail, maxItems, tailSleepInterval, lagFrequency = a.MonitoringParams(log)
                    let checkpointConfig = CosmosFeedConfig.Persistent (groupName, startFromTail, maxItems, lagFrequency)
                    SourceConfig.Cosmos (monitored, leases, checkpointConfig, tailSleepInterval)
                let context = client |> CosmosStoreContext.create
                let store = Config.Store.Cosmos (context, cache)
                store, buildSourceConfig, store
            | Choice2Of3 a ->
                let context = a.Connect()
                let buildSourceConfig log groupName =
                    let indexStore, startFromTail, batchSizeCutoff, tailSleepInterval, streamsDop = a.MonitoringParams(log)
                    let checkpoints = a.CreateCheckpointStore(groupName, cache)
                    let load = DynamoLoadModeConfig.Hydrate (context, streamsDop)
                    SourceConfig.Dynamo (indexStore, checkpoints, load, startFromTail, batchSizeCutoff, tailSleepInterval, x.StatsInterval)
                let store = Config.Store.Dynamo (context, cache)
                store, buildSourceConfig, store
            | Choice3Of3 a ->
                let connection = a.Connect(Log.Logger, AppName, EventStore.Client.NodePreference.Leader)
                let context = connection |> EventStoreContext.create
                let store = Config.Store.Esdb (context, cache)
                let targetStore = a.ConnectTarget(cache)
                let buildSourceConfig log groupName =
                    let startFromTail, maxItems, tailSleepInterval = a.MonitoringParams(log)
                    let checkpoints = a.CreateCheckpointStore(groupName, targetStore)
                    let hydrateBodies = true
                    SourceConfig.Esdb (connection.ReadConnection, checkpoints, hydrateBodies, startFromTail, maxItems, tailSleepInterval, x.StatsInterval)
                store, buildSourceConfig, targetStore

    /// Parse the commandline; can throw exceptions in response to missing arguments and/or `-h`/`--help` args
    let parse tryGetConfigValue argv : Arguments =
        let programName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name
        let parser = ArgumentParser.Create<Parameters>(programName = programName)
        Arguments(SourceArgs.Configuration tryGetConfigValue, parser.ParseCommandLine argv)

open Propulsion.Internal // AwaitKeyboardInterruptAsTaskCanceledException

let build (args : Args.Arguments) =
    let consumerGroupName, maxReadAhead, maxConcurrentStreams = args.ProcessorParams()
    let store, buildSourceConfig, targetStore = args.ConnectStoreSourceAndTarget()
    let log = Log.forGroup consumerGroupName // needs to have a `group` tag for Propulsion.Streams Prometheus metrics
    let filter, handle = Reactor.Config.create (store, targetStore)
    let stats = Reactor.Stats(log, args.StatsInterval, args.StateInterval, args.StoreVerbose, logExternalStats = args.DumpStoreMetrics)
    let sink = Reactor.Config.StartSink(log, stats, handle, maxReadAhead, maxConcurrentStreams,
                                        wakeForResults = args.WakeForResults, idleDelay = args.IdleDelay, purgeInterval = args.PurgeInterval)
    let source, _awaitSource =
        let sourceConfig = buildSourceConfig log consumerGroupName
        Reactor.Config.StartSource(log, sink, sourceConfig, filter)
    sink, source

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
