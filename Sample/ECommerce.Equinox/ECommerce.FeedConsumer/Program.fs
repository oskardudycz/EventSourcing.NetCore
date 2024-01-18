module ECommerce.FeedConsumer.Program

open Serilog
open System

type Configuration(tryGet) =
    inherit Args.Configuration(tryGet)

    let get key =                           match tryGet key with Some value -> value | None -> failwith $"Missing Argument/Environment Variable %s{key}"
    member _.BaseUri =                      get "API_BASE_URI"
    member _.Group =                        get "API_CONSUMER_GROUP"

let [<Literal>] AppName = "FeedConsumer"

module Args =

    open Argu

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-p"; Unique>]   PrometheusPort of int

        | [<AltCommandLine "-g"; Unique>]   Group of string
        | [<AltCommandLine "-f"; Unique>]   BaseUri of string

        | [<AltCommandLine "-r"; Unique>]   MaxReadAhead of int
        | [<AltCommandLine "-w"; Unique>]   FcsDop of int
        | [<AltCommandLine "-t"; Unique>]   TicketsDop of int

        | [<CliPrefix(CliPrefix.None); Unique; Last>] Cosmos of ParseResults<Args.Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique; Last>] Dynamo of ParseResults<Args.Dynamo.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique; Last>] Esdb of ParseResults<Args.Esdb.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique; Last>] Sss of ParseResults<Args.Sss.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "request verbose logging."
                | PrometheusPort _ ->       "port from which to expose a Prometheus /metrics endpoint. Default: off (optional if environment variable PROMETHEUS_PORT specified)"
                | Group _ ->                "specify Api Consumer Group Id. (optional if environment variable API_CONSUMER_GROUP specified)"
                | BaseUri _ ->              "specify Api endpoint. (optional if environment variable API_BASE_URI specified)"
                | MaxReadAhead _ ->         "maximum number of batches to let processing get ahead of completion. Default: 8."
                | FcsDop _ ->               "maximum number of FCs to process in parallel. Default: 4"
                | TicketsDop _ ->           "maximum number of Tickets to process in parallel (per FC). Default: 4"
                | Cosmos _ ->               "specify CosmosDB input parameters"
                | Dynamo _ ->               "specify DynamoDB input parameters"
                | Esdb _ ->                 "specify EventStore input parameters"
                | Sss _ ->                  "specify SqlStreamStore input parameters"

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        member val Verbose =                a.Contains Verbose
        member val PrometheusPort =         a.TryGetResult PrometheusPort |> Option.orElseWith (fun () -> c.PrometheusPort)
        member val CacheSizeMb =            10
        member val SourceId =               a.TryGetResult Group        |> Option.defaultWith (fun () -> c.Group) |> Propulsion.Feed.SourceId.parse
        member val BaseUri =                a.TryGetResult BaseUri      |> Option.defaultWith (fun () -> c.BaseUri) |> Uri
        member val MaxReadAhead =           a.GetResult(MaxReadAhead,8)
        member val FcsDop =                 a.TryGetResult FcsDop       |> Option.defaultValue 4
        member val TicketsDop =             a.TryGetResult TicketsDop   |> Option.defaultValue 4
        member val StatsInterval =          TimeSpan.FromMinutes 1.
        member val StateInterval =          TimeSpan.FromMinutes 5.
        member val CheckpointInterval =     TimeSpan.FromHours 1.
        member val TailSleepInterval =      TimeSpan.FromSeconds 1.
        member val ConsumerGroupName =      "default"
        member val StoreArgs : Args.StoreArgs =
            match a.TryGetSubCommand() with
            | Some (Parameters.Cosmos cosmos) -> Args.StoreArgs.Cosmos (Args.Cosmos.Arguments(c, cosmos))
            | Some (Parameters.Dynamo dynamo) -> Args.StoreArgs.Dynamo (Args.Dynamo.Arguments(c, dynamo))
            | Some (Parameters.Esdb es) ->       Args.StoreArgs.Esdb   (Args.Esdb.Arguments(c, es))
            | _ -> a.Raise "Must specify one of cosmos, dynamo or esdb for store"
        member x.VerboseStore =             Args.StoreArgs.verboseRequested x.StoreArgs
        member x.DumpStoreMetrics =         Args.StoreArgs.dumpMetrics x.StoreArgs
        member x.Connect() : Store.Config * Propulsion.Feed.IFeedCheckpointStore =
            let cache = Equinox.Cache(AppName, sizeMb = x.CacheSizeMb)
            let createCheckpoints = Args.Checkpoints.create (x.ConsumerGroupName, x.CheckpointInterval) Store.Metrics.log
            match x.StoreArgs with
            | Args.StoreArgs.Cosmos a ->
                let context = a.Connect() |> Async.RunSynchronously
                let store = Store.Config.Cosmos (context, cache)
                store, createCheckpoints (Args.Checkpoints.Config.Cosmos (context, cache))
            | Args.StoreArgs.Dynamo a ->
                let context = a.Connect()
                let store = Store.Config.Dynamo (context, cache)
                store, createCheckpoints (Args.Checkpoints.Config.Dynamo (context, cache))
            | Args.StoreArgs.Esdb a ->
                let context = a.Connect(Log.Logger, AppName, EventStore.Client.NodePreference.Leader) |> EventStoreContext.create
                let store = Store.Config.Esdb (context, cache)
                let checkpointStore = a.ConnectCheckpointStore(cache)
                store, createCheckpoints checkpointStore
            | Args.StoreArgs.Sss a ->
                let context = a.Connect() |> SqlStreamStoreContext.create
                let store = Store.Config.Sss (context, cache)
                let checkpointStore = a.CreateCheckpointStoreSql(x.ConsumerGroupName)
                store, checkpointStore

    /// Parse the commandline; can throw exceptions in response to missing arguments and/or `-h`/`--help` args
    let parse tryGetConfigValue argv =
        let programName = Reflection.Assembly.GetEntryAssembly().GetName().Name
        let parser = ArgumentParser.Create<Parameters>(programName = programName)
        Arguments(Configuration tryGetConfigValue, parser.ParseCommandLine argv)

let build (args : Args.Arguments) =
    let _store, checkpoints = args.Connect() // TODO wireup to use store in handler

    let log = Log.forGroup args.SourceId // needs to have a `group` tag for Propulsion.Streams Prometheus metrics
    let sink =
        let handle = Ingester.handle args.TicketsDop
        let stats = Ingester.Stats(log, args.StatsInterval, args.StateInterval, logExternalStats = args.DumpStoreMetrics)
        Propulsion.Sinks.Factory.StartConcurrent(log, args.MaxReadAhead, args.FcsDop, handle, stats)
    let pumpSource =
        let feed = ApiClient.TicketsFeed args.BaseUri
        let source =
            Propulsion.Feed.FeedSource(
                log, args.StatsInterval, args.SourceId, args.TailSleepInterval,
                checkpoints, sink)
        source.Start(feed.ReadTranches, fun t p -> feed.Poll(t, p))
    sink, pumpSource

open Propulsion.Internal // AwaitKeyboardInterruptAsTaskCanceledException

let run args = async {
    let sink, source = build args
    use _ = args.PrometheusPort |> Option.map startMetricsServer |> Option.toObj
    return! [|  Async.AwaitKeyboardInterruptAsTaskCanceledException()
                source.AwaitWithStopOnCancellation()
                sink.AwaitWithStopOnCancellation()
            |] |> Async.Parallel |> Async.Ignore<unit array> }

[<EntryPoint>]
let main argv =
    try let args = Args.parse EnvVar.tryGet argv
        try let metrics = Sinks.equinoxAndPropulsionFeedConsumerMetrics (Sinks.tags AppName)
            Log.Logger <- LoggerConfiguration().Configure(args.Verbose).Sinks(metrics, args.VerboseStore).CreateLogger()
            try run args |> Async.RunSynchronously; 0
            with e when not (e :? System.Threading.Tasks.TaskCanceledException) -> Log.Fatal(e, "Exiting"); 2
        finally Log.CloseAndFlush()
    with:? Argu.ArguParseException as e -> eprintfn $"%s{e.Message}"; 1
        | e -> eprintfn $"Exception %s{e.Message}"; 1
