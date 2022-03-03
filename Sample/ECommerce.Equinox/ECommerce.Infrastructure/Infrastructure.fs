[<AutoOpen>]
module ECommerce.Infrastructure

open Serilog
open System

module Log =

    let isStoreMetrics x = Serilog.Filters.Matching.WithProperty("isMetric").Invoke x
    let forGroup group = Log.ForContext("group", group)

module EnvVar =

    let tryGet varName : string option = Environment.GetEnvironmentVariable varName |> Option.ofObj

type Equinox.CosmosStore.CosmosStoreConnector with

    member private x.LogConfiguration(connectionName, databaseId, containerId) =
        let o = x.Options
        let timeout, retries429, timeout429 = o.RequestTimeout, o.MaxRetryAttemptsOnRateLimitedRequests, o.MaxRetryWaitTimeOnRateLimitedRequests
        Log.Information("CosmosDb {name} {mode} {endpointUri} timeout {timeout}s; Throttling retries {retries}, max wait {maxRetryWaitTime}s",
                        connectionName, o.ConnectionMode, x.Endpoint, timeout.TotalSeconds, retries429, let t = timeout429.Value in t.TotalSeconds)
        Log.Information("CosmosDb {name} Database {database} Container {container}",
                        connectionName, databaseId, containerId)

    /// Use sparingly; in general one wants to use CreateAndInitialize to avoid slow first requests
    member x.CreateUninitialized(databaseId, containerId) =
        x.CreateUninitialized().GetDatabase(databaseId).GetContainer(containerId)

    /// Connect a CosmosStoreClient, including warming up
    member x.ConnectStore(connectionName, databaseId, containerId) =
        x.LogConfiguration(connectionName, databaseId, containerId)
        Equinox.CosmosStore.CosmosStoreClient.Connect(x.CreateAndInitialize, databaseId, containerId)

    /// Creates a CosmosClient suitable for running a CFP via CosmosStoreSource
    member x.ConnectMonitored(databaseId, containerId, ?connectionName) =
        x.LogConfiguration(defaultArg connectionName "Source", databaseId, containerId)
        x.CreateUninitialized(databaseId, containerId)

    /// Connects to a Store as both a ChangeFeedProcessor Monitored Container and a CosmosStoreClient
    member x.ConnectStoreAndMonitored(databaseId, containerId) =
        let monitored = x.ConnectMonitored(databaseId, containerId, "Main")
        let storeClient = Equinox.CosmosStore.CosmosStoreClient(monitored.Database.Client, databaseId, containerId)
        storeClient, monitored

module CosmosStoreContext =

    /// Create with default packing and querying policies. Search for other `module CosmosStoreContext` impls for custom variations
    let create (storeClient : Equinox.CosmosStore.CosmosStoreClient) =
        let maxEvents = 256
        Equinox.CosmosStore.CosmosStoreContext(storeClient, tipMaxEvents=maxEvents)

module EventStoreContext =

    let create (storeClient : Equinox.EventStore.EventStoreConnection) =
        let batchingPolicy = Equinox.EventStore.BatchingPolicy(maxBatchSize = 200)
        Equinox.EventStore.EventStoreContext(storeClient, batchingPolicy)

/// Equinox and Propulsion provide metrics as properties in log emissions
/// These helpers wire those to pass through virtual Log Sinks that expose them as Prometheus metrics.
module Sinks =

    let tags appName = ["app", appName]

    let equinoxMetricsOnly tags (l : LoggerConfiguration) =
        l.WriteTo.Sink(Equinox.CosmosStore.Core.Log.InternalMetrics.Stats.LogSink())
         .WriteTo.Sink(Equinox.CosmosStore.Prometheus.LogSink(tags))

    let equinoxAndPropulsionConsumerMetrics tags (l : LoggerConfiguration) =
        l |> equinoxMetricsOnly tags
          |> fun l -> l.WriteTo.Sink(Propulsion.Prometheus.LogSink(tags))

    let equinoxAndPropulsionCosmosConsumerMetrics tags (l : LoggerConfiguration) =
        l |> equinoxAndPropulsionConsumerMetrics tags
          |> fun l -> l.WriteTo.Sink(Propulsion.CosmosStore.Prometheus.LogSink(tags))

    let equinoxAndPropulsionFeedConsumerMetrics tags (l : LoggerConfiguration) =
        l |> equinoxAndPropulsionConsumerMetrics tags
          |> fun l -> l.WriteTo.Sink(Propulsion.Feed.Prometheus.LogSink(tags))

    let console (configuration : LoggerConfiguration) =
        let t = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}"
        configuration.WriteTo.Console(theme=Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code, outputTemplate=t)

[<System.Runtime.CompilerServices.Extension>]
type Logging() =

    [<System.Runtime.CompilerServices.Extension>]
    static member Configure(configuration : LoggerConfiguration, ?verbose) =
        configuration
            .Destructure.FSharpTypes()
            .Enrich.FromLogContext()
        |> fun c -> if verbose = Some true then c.MinimumLevel.Debug() else c

    [<System.Runtime.CompilerServices.Extension>]
    static member private Sinks(configuration : LoggerConfiguration, configureMetricsSinks, configureConsoleSink, ?isMetric) =
        let configure (a : Configuration.LoggerSinkConfiguration) : unit =
            a.Logger(configureMetricsSinks >> ignore) |> ignore // unconditionally feed all log events to the metrics sinks
            a.Logger(fun l -> // but filter what gets emitted to the console sink
                let l = match isMetric with None -> l | Some predicate -> l.Filter.ByExcluding(Func<Serilog.Events.LogEvent, bool> predicate)
                configureConsoleSink l |> ignore)
            |> ignore
        configuration.WriteTo.Async(bufferSize=65536, blockWhenFull=true, configure=System.Action<_> configure)

    [<System.Runtime.CompilerServices.Extension>]
    static member Sinks(configuration : LoggerConfiguration, configureMetricsSinks, verboseStore) =
        configuration.Sinks(configureMetricsSinks, Sinks.console, ?isMetric = if verboseStore then None else Some Log.isStoreMetrics)

/// A typical app will likely have health checks etc, implying the wireup would be via `UseMetrics()` and thus not use this ugly code directly
let startMetricsServer port : IDisposable =
    let metricsServer = new Prometheus.KestrelMetricServer(port = port)
    let ms = metricsServer.Start()
    Log.Information("Prometheus /metrics endpoint on port {port}", port)
    { new IDisposable with member x.Dispose() = ms.Stop(); (metricsServer :> IDisposable).Dispose() }
