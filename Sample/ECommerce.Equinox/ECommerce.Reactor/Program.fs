module ECommerce.Reactor.Program

open ECommerce.Infrastructure
open Serilog
open System

module Args =

    open Argu
    open ECommerce.Reactor.Args

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-p"; Unique>]   PrometheusPort of int
        | [<AltCommandLine "-g"; Mandatory>] ProcessorName of string
        | [<AltCommandLine "-r"; Unique>]   MaxReadAhead of int
        | [<AltCommandLine "-w"; Unique>]   MaxWriters of int
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<CosmosSource.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Dynamo of ParseResults<DynamoSource.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Esdb of ParseResults<EsdbSource.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "request Verbose Logging. Default: off."
                | PrometheusPort _ ->       "port from which to expose a Prometheus /metrics endpoint. Default: off (optional if environment variable PROMETHEUS_PORT specified)"
                | ProcessorName _ ->        "Projector consumer group name."
                | MaxReadAhead _ ->         "maximum number of batches to let processing get ahead of completion. Default: 16."
                | MaxWriters _ ->           "maximum number of concurrent streams on which to process at any time. Default: 8."
                | Cosmos _ ->               "specify CosmosDB input parameters."
                | Dynamo _ ->               "specify DynamoDB input parameters."
                | Esdb _ ->                 "specify EventStoreDB input parameters."
    and Arguments(c : Configuration, a : ParseResults<Parameters>) =
        member val Verbose =                a.Contains Verbose
        member val PrometheusPort =         a.TryGetResult PrometheusPort |> Option.orElseWith (fun () -> c.PrometheusPort)
        member val ProcessorName =          a.GetResult ProcessorName
        member val MaxReadAhead =           a.GetResult(MaxReadAhead, 16)
        member val MaxConcurrentStreams =   a.GetResult(MaxWriters, 8)
        // 1ms -> 10ms reduces CPU consumption from ~5s/min to .7s/min
        member val IdleDelay =              TimeSpan.FromMilliseconds 10.
        member val StatsInterval =          TimeSpan.FromMinutes 1.
        member val StateInterval =          TimeSpan.FromMinutes 5.
        member val Source : Choice<CosmosSource.Arguments, DynamoSource.Arguments, EsdbSource.Arguments> =
                                            match a.GetSubCommand() with
                                            | Cosmos a -> Choice1Of3 <| CosmosSource.Arguments(c, a)
                                            | Dynamo a -> Choice2Of3 <| DynamoSource.Arguments(c, a)
                                            | Esdb a -> Choice3Of3 <| EsdbSource.Arguments(c, a)
                                            | a -> Args.missingArg $"Unexpected Store subcommand %A{a}"
        member x.StoreVerbose =             match x.Source with
                                            | Choice1Of3 s -> s.Verbose
                                            | Choice2Of3 s -> s.Verbose
                                            | Choice3Of3 s -> s.Verbose

    /// Parse the commandline; can throw exceptions in response to missing arguments and/or `-h`/`--help` args
    let parse tryGetConfigValue argv : Arguments =
        let programName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name
        let parser = ArgumentParser.Create<Parameters>(programName=programName)
        Arguments(Configuration tryGetConfigValue, parser.ParseCommandLine argv)

let [<Literal>] AppName = "ECommerce.Reactor"

open Propulsion.CosmosStore.Infrastructure // AwaitKeyboardInterruptAsTaskCancelledException
open ECommerce.Domain


let build (args : Args.Arguments) =

    match args.Source with
    | Choice1Of2 cosmos (context, monitored, leases, processorName, startFromTail, maxItems, lagFrequency) ->
        let cache = Equinox.Cache(AppName, sizeMb = 10)
        let store = Config.Store.Cosmos (context, cache)
        let handle = Reactor.Config.create (store, store)
        let stats = Reactor.Stats(Log.Logger, args.StatsInterval, args.StateInterval)
        let sink = Propulsion.Streams.StreamsProjector.Start(Log.Logger, args.MaxReadAhead, args.MaxConcurrentStreams, handle, stats, args.StatsInterval)
        let source =
            let mapToStreamItems = Seq.collect Propulsion.CosmosStore.EquinoxSystemTextJsonParser.enumStreamEvents
            let observer = Propulsion.CosmosStore.CosmosStoreSource.CreateObserver(Log.Logger, sink.StartIngester, mapToStreamItems)
            Propulsion.CosmosStore.CosmosStoreSource.Start(Log.Logger, monitored, leases, processorName, observer, startFromTail, ?maxItems=maxItems, lagReportFreq=lagFrequency)
        [ Async.AwaitKeyboardInterruptAsTaskCancelledException()
          source.AwaitWithStopOnCancellation()
          sink.AwaitWithStopOnCancellation() ]
    | Choice1Of2 (srcE, context, spec) ->
        let connectEs () = srcE.Connect(Log.Logger, Log.Logger, AppName, Equinox.EventStoreDb.ConnectionStrategy.ClusterSingle EventStore.Client.NodePreference.Leader)
        let connectProjEs () = srcE.ConnectProj(Log.Logger, Log.Logger, AppName, EventStore.Client.NodePreference.Follower)

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

let run (args : Args.Arguments) = async {
    use _ = args.PrometheusPort |> Option.map startMetricsServer |> Option.toObj
    return! Async.Parallel (build args) |> Async.Ignore<unit array> }

[<EntryPoint>]
let main argv =
    try let args = Args.parse EnvVar.tryGet argv
        let metrics = Sinks.tags AppName |> Sinks.equinoxAndPropulsionReactorMetrics
        try Log.Logger <- LoggerConfiguration().Configure(verbose=args.Verbose).Sinks(metrics, args.StoreVerbose).CreateLogger()
            try run args |> Async.RunSynchronously; 0
            with e when not (e :? Args.MissingArg) -> Log.Fatal(e, "Exiting"); 2
        finally Log.CloseAndFlush()
    with Args.MissingArg msg -> eprintfn "%s" msg; 1
        | :? Argu.ArguParseException as e -> eprintfn "%s" e.Message; 1
        | e -> eprintf "Exception %s" e.Message; 1
