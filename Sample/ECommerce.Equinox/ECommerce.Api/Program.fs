module ECommerce.Api.Program

open ECommerce
open ECommerce.Infrastructure // Args etc
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Serilog
open System

type Configuration(tryGet) =
    inherit Args.Configuration(tryGet)

let [<Literal>] AppName = "ECommerce.Web"

module Args =

    open Argu

    type [<NoEquality; NoComparison>] Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-p"; Unique>]   PrometheusPort of int
        | [<CliPrefix(CliPrefix.None); Last>] Cosmos of ParseResults<Args.Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Last>] Dynamo of ParseResults<Args.Dynamo.Parameters>
        | [<CliPrefix(CliPrefix.None); Last>] Esdb of ParseResults<Args.Esdb.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose _ ->              "request verbose logging."
                | PrometheusPort _ ->       "port from which to expose a Prometheus /metrics endpoint. Default: off (optional if environment variable PROMETHEUS_PORT specified)"
                | Cosmos _ ->               "specify CosmosDB input parameters"
                | Dynamo _ ->               "specify DynamoDB input parameters"
                | Esdb _ ->                 "specify EventStore input parameters"
    and [<RequireQualifiedAccess>]
        Arguments(c : Configuration, a : ParseResults<Parameters>) =
        member val Verbose =                a.Contains Verbose
        member val PrometheusPort =         a.TryGetResult PrometheusPort |> Option.orElseWith (fun () -> c.PrometheusPort)
        member val CacheSizeMb =            10
        member val StoreArgs : Args.StoreArgs =
            match a.TryGetSubCommand() with
            | Some (Parameters.Cosmos cosmos) -> Args.StoreArgs.Cosmos (Args.Cosmos.Arguments(c, cosmos))
            | Some (Parameters.Dynamo dynamo) -> Args.StoreArgs.Dynamo (Args.Dynamo.Arguments(c, dynamo))
            | Some (Parameters.Esdb es) ->       Args.StoreArgs.Esdb (Args.Esdb.Arguments(c, es))
            | _ -> Args.missingArg "Must specify one of cosmos, dynamo or esdb for store"
        member x.StoreVerbose =             Args.StoreArgs.verboseRequested x.StoreArgs
        member x.Connect() : Domain.Config.Store<_> =
            let cache = Equinox.Cache (AppName, sizeMb = x.CacheSizeMb)
            match x.StoreArgs with
            | Args.StoreArgs.Cosmos a ->
                let context = a.Connect() |> Async.RunSynchronously |> CosmosStoreContext.create
                Domain.Config.Store.Cosmos (context, cache)
            | Args.StoreArgs.Dynamo a ->
                let context = a.Connect() |> DynamoStoreContext.create
                Domain.Config.Store.Dynamo (context, cache)
            | Args.StoreArgs.Esdb a ->
                let context = a.Connect(Log.Logger, AppName, EventStore.Client.NodePreference.Leader) |> EventStoreContext.create
                Domain.Config.Store.Esdb (context, cache)

    /// Parse the commandline; can throw exceptions in response to missing arguments and/or `-h`/`--help` args
    let parse tryGetConfigValue argv =
        let programName = Reflection.Assembly.GetEntryAssembly().GetName().Name
        let parser = ArgumentParser.Create<Parameters>(programName = programName)
        Arguments(Configuration tryGetConfigValue, parser.ParseCommandLine argv)

let run (args : Args.Arguments) =
    let store = args.Connect()
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
        let metrics = Sinks.tags AppName |> Sinks.equinoxMetricsOnly
        try Log.Logger <- LoggerConfiguration()
                .Configure(args.Verbose)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .Sinks(metrics, args.StoreVerbose)
                .CreateLogger()
            try run args; 0
            with e when not (e :? Args.MissingArg) -> Log.Fatal(e, "Exiting"); 2
        finally Log.CloseAndFlush()
    with Args.MissingArg msg -> eprintfn $"%s{msg}"; 1
        | :? Argu.ArguParseException as e -> eprintfn $"%s{e.Message}"; 1
        | e -> eprintfn "Exception %s" e.Message; 1
