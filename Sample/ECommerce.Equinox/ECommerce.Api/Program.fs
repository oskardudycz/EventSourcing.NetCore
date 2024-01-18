module ECommerce.Api.Program

open ECommerce
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
        | [<CliPrefix(CliPrefix.None); Last>] Sss of ParseResults<Args.Sss.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "request verbose logging."
                | PrometheusPort _ ->       "port from which to expose a Prometheus /metrics endpoint. Default: off (optional if environment variable PROMETHEUS_PORT specified)"
                | Cosmos _ ->               "specify CosmosDB input parameters"
                | Dynamo _ ->               "specify DynamoDB input parameters"
                | Esdb _ ->                 "specify EventStore input parameters"
                | Sss _ ->                  "specify SqlStreamStore input parameters"
    and [<RequireQualifiedAccess>]
        Arguments(c : Configuration, p : ParseResults<Parameters>) =
        member val Verbose =                p.Contains Verbose
        member val PrometheusPort =         p.TryGetResult PrometheusPort |> Option.orElseWith (fun () -> c.PrometheusPort)
        member val CacheSizeMb =            10
        member val StoreArgs : Args.StoreArgs =
            match p.TryGetSubCommand() with
            | Some (Parameters.Cosmos cosmos) -> Args.StoreArgs.Cosmos (Args.Cosmos.Arguments(c, cosmos))
            | Some (Parameters.Dynamo dynamo) -> Args.StoreArgs.Dynamo (Args.Dynamo.Arguments(c, dynamo))
            | Some (Parameters.Esdb es) ->       Args.StoreArgs.Esdb (Args.Esdb.Arguments(c, es))
            | Some (Parameters.Sss sss) ->       Args.StoreArgs.Sss (Args.Sss.Arguments(c, sss))
            | _ -> p.Raise "Must specify one of cosmos, dynamo, esdb or sss for store"
        member x.VerboseStore =             Args.StoreArgs.verboseRequested x.StoreArgs
        member x.Connect(): Store.Config =
            let cache = Equinox.Cache (AppName, sizeMb = x.CacheSizeMb)
            Args.StoreArgs.connectTarget x.StoreArgs cache

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
                .Sinks(metrics, args.VerboseStore)
                .CreateLogger()
            try run args; 0
            with e -> Log.Fatal(e, "Exiting"); 2
        finally Log.CloseAndFlush()
    with:? Argu.ArguParseException as e -> eprintfn $"%s{e.Message}"; 1
        | e -> eprintfn $"Exception %s{e.Message}"; 1
