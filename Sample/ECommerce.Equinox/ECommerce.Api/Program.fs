module ECommerce.Api.Program

open ECommerce
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Serilog
open System

exception MissingArg of message : string with override this.Message = this.message

type Configuration(tryGet) =

    let get key =
        match tryGet key with
        | Some value -> value
        | None -> raise (MissingArg $"Missing Argument/Environment Variable %s{key}")

    member _.CosmosConnection =             get "EQUINOX_COSMOS_CONNECTION"
    member _.CosmosDatabase =               get "EQUINOX_COSMOS_DATABASE"
    member _.CosmosContainer =              get "EQUINOX_COSMOS_CONTAINER"

    member _.EventStoreHost =               get "EQUINOX_ES_HOST"
    member _.EventStorePort =               tryGet "EQUINOX_ES_PORT" |> Option.map int
    member _.EventStoreUsername =           get "EQUINOX_ES_USERNAME"
    member _.EventStorePassword =           get "EQUINOX_ES_PASSWORD"

module Args =

    open Argu
    open Equinox.EventStore
    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<CliPrefix(CliPrefix.None); Last>] Cosmos of ParseResults<CosmosParameters>
        | [<CliPrefix(CliPrefix.None); Last>] Esdb of ParseResults<EsdbParameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose _ ->              "request verbose logging."
                | Cosmos _ ->               "specify CosmosDb input parameters"
                | Esdb _ ->                 "specify EventStore input parameters"
    and StoreArguments = Cosmos of CosmosArguments | Esdb of EsdbArguments
    and [<RequireQualifiedAccess>] Arguments(c : Configuration, a : ParseResults<Parameters>) =
        member val Store : StoreArguments =
            match a.TryGetSubCommand() with
            | Some (Parameters.Esdb es) -> StoreArguments.Esdb (EsdbArguments (c, es))
            | Some (Parameters.Cosmos cosmos) -> StoreArguments.Cosmos (CosmosArguments (c, cosmos))
            | _ -> raise (MissingArg "Must specify one of cosmos or esdb for store")
        member x.StoreVerbose =
            match x.Store with
            | StoreArguments.Cosmos a -> a.Verbose
            | StoreArguments.Esdb a -> a.Verbose
        member val Verbose =                a.Contains Parameters.Verbose
    and [<NoEquality; NoComparison>] CosmosParameters =
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
                | Verbose ->                "request Verbose Logging from Store. Default: off"
                | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                | Database _ ->             "specify a database name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                | Container _ ->            "specify a container name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_CONTAINER specified)"
                | Timeout _ ->              "specify operation timeout in seconds (default: 5)."
                | Retries _ ->              "specify operation retries (default: 1)."
                | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds (default: 5)"
    and CosmosArguments(c : Configuration, a : ParseResults<CosmosParameters>) =
        let discovery =                     a.TryGetResult Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
        let mode =                          a.TryGetResult ConnectionMode
        let timeout =                       a.GetResult(CosmosParameters.Timeout, 5.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(CosmosParameters.Retries, 1)
        let maxRetryWaitTime =              a.GetResult(RetriesWaitTime, 5.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
        let database =                      a.TryGetResult Database |> Option.defaultWith (fun () -> c.CosmosDatabase)
        let container =                     a.TryGetResult Container |> Option.defaultWith (fun () -> c.CosmosContainer)
        member _.Verbose =                  a.Contains CosmosParameters.Verbose
        member _.Connect() =                connector.ConnectStore("Main", database, container)
    and [<NoEquality; NoComparison>] EsdbParameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-oh">]          HeartbeatTimeout of float
        | [<AltCommandLine "-h">]           Host of string
        | [<AltCommandLine "-x">]           Port of int
        | [<AltCommandLine "-u">]           Username of string
        | [<AltCommandLine "-p">]           Password of string
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | Host _ ->                 "TCP mode: specify EventStore hostname to connect to directly. Clustered mode: use Gossip protocol against all A records returned from DNS query. (optional if environment variable EQUINOX_ES_HOST specified)"
                | Port _ ->                 "specify EventStore custom port. Uses value of environment variable EQUINOX_ES_PORT if specified. Defaults for Cluster and Direct TCP/IP mode are 30778 and 1113 respectively."
                | Username _ ->             "specify username for EventStore. (optional if environment variable EQUINOX_ES_USERNAME specified)"
                | Password _ ->             "specify Password for EventStore. (optional if environment variable EQUINOX_ES_PASSWORD specified)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."
                | HeartbeatTimeout _ ->     "specify heartbeat timeout in seconds. Default: 1.5."
    and EsdbArguments(c : Configuration, a : ParseResults<EsdbParameters>) =
        let discovery (host, port) =
            match port with
            | None ->   Discovery.GossipDns            host
            | Some p -> Discovery.GossipDnsCustomPort (host, p)
        member val Port =                   match a.TryGetResult Port with Some x -> Some x | None -> c.EventStorePort
        member val Host =                   a.TryGetResult Host     |> Option.defaultWith (fun () -> c.EventStoreHost)
        member val User =                   a.TryGetResult Username |> Option.defaultWith (fun () -> c.EventStoreUsername)
        member val Password =               a.TryGetResult Password |> Option.defaultWith (fun () -> c.EventStorePassword)
        member val Retries =                a.GetResult(EsdbParameters.Retries, 3)
        member val Timeout =                a.GetResult(EsdbParameters.Timeout, 20.) |> TimeSpan.FromSeconds
        member val Heartbeat =              a.GetResult(HeartbeatTimeout, 1.5) |> TimeSpan.FromSeconds

        member _.Verbose =                  a.Contains EsdbParameters.Verbose
        member x.Connect(log: ILogger, storeLog: ILogger, appName, nodePreference) =
            let discovery = discovery (x.Host, x.Port)
            let ts (x : TimeSpan) = x.TotalSeconds
            log.ForContext("host", x.Host).ForContext("port", x.Port)
                .Information("EventStore {discovery} heartbeat: {heartbeat}s Timeout: {timeout}s Retries {retries}",
                    discovery, ts x.Heartbeat, ts x.Timeout, x.Retries)
            let log=if storeLog.IsEnabled Serilog.Events.LogEventLevel.Debug then Logger.SerilogVerbose storeLog else Logger.SerilogNormal storeLog
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Connector(x.User, x.Password, x.Timeout, x.Retries, log=log, heartbeatTimeout=x.Heartbeat, tags=tags)
                .Establish(appName, discovery, ConnectionStrategy.ClusterSingle nodePreference) |> Async.RunSynchronously

    /// Parse the commandline; can throw exceptions in response to missing arguments and/or `-h`/`--help` args
    let parse tryGetConfigValue argv =
        let programName = Reflection.Assembly.GetEntryAssembly().GetName().Name
        let parser = ArgumentParser.Create<Parameters>(programName=programName)
        Arguments(Configuration tryGetConfigValue, parser.ParseCommandLine argv)

[<System.Runtime.CompilerServices.Extension>]
type Logging() =

    [<System.Runtime.CompilerServices.Extension>]
    static member Configure(c : LoggerConfiguration, appName) =
        let customTags = ["app",appName]
        c
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Sink(Equinox.CosmosStore.Prometheus.LogSink(customTags))
            .Enrich.FromLogContext()
            .WriteTo.Console()

let [<Literal>] AppName = "ECommerce.Web"

let run (args : Args.Arguments) =
    let store =
        match args.Store with
        | Args.StoreArguments.Cosmos ca ->
            let context = ca.Connect() |> Async.RunSynchronously |> CosmosStoreContext.create
            let cache = Equinox.Cache (AppName, sizeMb = 10)
            Domain.Config.Store.Cosmos (context, cache)
        | Args.StoreArguments.Esdb ea ->
            let context = ea.Connect(Log.Logger, Log.Logger, AppName, Equinox.EventStore.NodePreference.PreferMaster) |> EventStoreContext.create
            let cache = Equinox.Cache (AppName, sizeMb = 10)
            Domain.Config.Store.Esdb (context, cache)
    let carts = Domain.ShoppingCart.Config.create store
    let registerServices (services: IServiceCollection) =
        services.AddSingleton(carts) |> ignore
    WebHostBuilder()
        .UseKestrel()
        .UseSerilog()
        .ConfigureServices(registerServices)
        .UseStartup<Startup>()
        .Build()
        .Run()

[<EntryPoint>]
let main argv =
    try let args = Args.parse EnvVar.tryGet argv
        try Log.Logger <- LoggerConfiguration().Configure(args.Verbose).Sinks(Sinks.tags AppName |> Sinks.equinoxMetricsOnly, args.StoreVerbose).CreateLogger()
            try run args; 0
            with e when not (e :? MissingArg) -> Log.Fatal(e, "Exiting"); 2
        finally Log.CloseAndFlush()
    with MissingArg msg -> eprintfn $"%s{msg}"; 1
        | :? Argu.ArguParseException as e -> eprintfn $"%s{e.Message}"; 1
        | e -> eprintfn $"Exception %s{e.Message}"; 1
