[<AutoOpen>]
module Helpers

open Serilog
open System

module Log =

    let isStoreMetrics x = Serilog.Filters.Matching.WithProperty("isMetric").Invoke x
    let forGroup group = Log.ForContext("group", group)

module EnvVar =

    let tryGet varName : string option = Environment.GetEnvironmentVariable varName |> Option.ofObj

type Equinox.CosmosStore.CosmosStoreContext with

    member x.LogConfiguration(role, databaseId: string, containerId: string) =
        Log.Information("CosmosStore {role:l} {db}/{container} Tip maxEvents {maxEvents} maxSize {maxJsonLen} Query maxItems {queryMaxItems}",
                        role, databaseId, containerId, x.TipOptions.MaxEvents, x.TipOptions.MaxJsonLength, x.QueryOptions.MaxItems)

type Equinox.CosmosStore.CosmosStoreClient with

    member x.CreateContext(role: string, databaseId, containerId, tipMaxEvents, ?queryMaxItems, ?tipMaxJsonLength, ?skipLog) =
        let c = Equinox.CosmosStore.CosmosStoreContext(x, databaseId, containerId, tipMaxEvents, ?queryMaxItems = queryMaxItems, ?tipMaxJsonLength = tipMaxJsonLength)
        if skipLog = Some true then () else c.LogConfiguration(role, databaseId, containerId)
        c

module CosmosStoreConnector =

    let private get (role: string) (client: Microsoft.Azure.Cosmos.CosmosClient) databaseId containerId =
        Log.Information("CosmosDB {role} Database {database} Container {container}", role, databaseId, containerId)
        client.GetDatabase(databaseId).GetContainer(containerId)
    let getSource c = get "Source" c
    let getLeases c = get "Leases" c
    let getSourceAndLeases client databaseId containerId auxContainerId =
        getSource client databaseId containerId, getLeases client databaseId auxContainerId

type Equinox.CosmosStore.CosmosStoreConnector with

    member private x.LogConfiguration(role, databaseId: string, containers: string[]) =
        let o = x.Options
        let timeout, retries429, timeout429 = o.RequestTimeout, o.MaxRetryAttemptsOnRateLimitedRequests, o.MaxRetryWaitTimeOnRateLimitedRequests
        Log.Information("CosmosDB {role} {mode} {endpointUri} {db} {containers} timeout {timeout}s Throttling retries {retries}, max wait {maxRetryWaitTime}s",
                        role, o.ConnectionMode, x.Endpoint, databaseId, containers, timeout.TotalSeconds, retries429, let t = timeout429.Value in t.TotalSeconds)
    member private x.CreateAndInitialize(role, databaseId, containers) =
        x.LogConfiguration(role, databaseId, containers)
        x.CreateAndInitialize(databaseId, containers)
    // member private x.Connect(role, databaseId, containers) =
    //     x.LogConfiguration(role, databaseId, containers)
    //     x.Connect(databaseId, containers)
    member private x.ConnectContexts(role, databaseId, containerId, ?auxContainerId): Async<_ * Equinox.CosmosStore.CosmosStoreContext> = async {
        let! cosmosClient = x.CreateAndInitialize(role, databaseId, [| yield containerId; yield! Option.toList auxContainerId |])
        let client = Equinox.CosmosStore.CosmosStoreClient(cosmosClient)
        let contexts = client.CreateContext(role, databaseId, containerId, tipMaxEvents = 256, queryMaxItems = 100)
        return cosmosClient, contexts }
    /// Connect to the database (including verifying and warming up relevant containers), establish relevant CosmosStoreContexts required by Domain
    member x.Connect(role, databaseId, containerId: string) = async {
        let! _client, contexts = x.ConnectContexts(role, databaseId, containerId)
        return contexts }
    member x.ConnectWithFeed(databaseId, containerId, auxContainerId) = async {
        let! client, context = x.ConnectContexts("Main", databaseId, containerId, auxContainerId)
        let source, leases = CosmosStoreConnector.getSourceAndLeases client databaseId containerId auxContainerId
        return context, source, leases }

type Equinox.DynamoStore.DynamoStoreConnector with

    member x.LogConfiguration() =
        Log.Information("DynamoStore {endpoint} Timeout {timeoutS}s Retries {retries}",
                        x.Endpoint, (let t = x.Timeout in t.TotalSeconds), x.Retries)

    member x.CreateClient() =
        x.LogConfiguration()
        x.CreateDynamoStoreClient()

type Equinox.DynamoStore.DynamoStoreClient with

    member x.CreateContext(role, table, ?queryMaxItems, ?maxBytes, ?archiveTableName: string) =
        let queryMaxItems = defaultArg queryMaxItems 100
        let c = Equinox.DynamoStore.DynamoStoreContext(x, table, queryMaxItems = queryMaxItems, ?maxBytes = maxBytes, ?archiveTableName = archiveTableName)
        Log.Information("DynamoStore {role:l} Table {table} Archive {archive} Tip thresholds: {maxTipBytes}b {maxTipEvents}e Query paging {queryMaxItems} items",
                        role, table, Option.toObj archiveTableName, c.TipOptions.MaxBytes, Option.toNullable c.TipOptions.MaxEvents, c.QueryOptions.MaxItems)
        c

type Equinox.DynamoStore.DynamoStoreContext with

    member context.CreateCheckpointService(consumerGroupName, cache, log, ?checkpointInterval) =
        let checkpointInterval = defaultArg checkpointInterval (TimeSpan.FromHours 1.)
        Propulsion.Feed.ReaderCheckpoint.DynamoStore.create log (consumerGroupName, checkpointInterval) (context, cache)

module EventStoreContext =

    let create (storeConnection : Equinox.EventStoreDb.EventStoreConnection) =
        Equinox.EventStoreDb.EventStoreContext(storeConnection, batchSize = 200)

module SqlStreamStoreContext =

    let create (storeConnection : Equinox.SqlStreamStore.SqlStreamStoreConnection) =
        Equinox.SqlStreamStore.SqlStreamStoreContext(storeConnection, batchSize = 200)

/// Equinox and Propulsion provide metrics as properties in log emissions
/// These helpers wire those to pass through virtual Log Sinks that expose them as Prometheus metrics.
module Sinks =

    let tags appName = ["app", appName]

    let equinoxMetricsOnly tags (l : LoggerConfiguration) =
        l.WriteTo.Sink(Equinox.CosmosStore.Core.Log.InternalMetrics.Stats.LogSink())
         .WriteTo.Sink(Equinox.CosmosStore.Prometheus.LogSink(tags))
         .WriteTo.Sink(Equinox.DynamoStore.Core.Log.InternalMetrics.Stats.LogSink())
         .WriteTo.Sink(Equinox.DynamoStore.Prometheus.LogSink(tags))
         .WriteTo.Sink(Equinox.EventStoreDb.Log.InternalMetrics.Stats.LogSink())

    let equinoxAndPropulsionMetrics tags (l : LoggerConfiguration) =
        l |> equinoxMetricsOnly tags
          |> _.WriteTo.Sink(Propulsion.Prometheus.LogSink(tags))

    let equinoxAndPropulsionReactorMetrics tags (l : LoggerConfiguration) =
        l |> equinoxAndPropulsionMetrics tags
          |> _.WriteTo.Sink(Propulsion.CosmosStore.Prometheus.LogSink(tags))
              .WriteTo.Sink(Propulsion.Feed.Prometheus.LogSink(tags)) // Esdb and Dynamo indirectly provide metrics via Feed

    let equinoxAndPropulsionFeedConsumerMetrics tags (l : LoggerConfiguration) =
        l |> equinoxAndPropulsionMetrics tags
          |> _.WriteTo.Sink(Propulsion.Feed.Prometheus.LogSink(tags))

    let console (configuration : LoggerConfiguration) =
        let t = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}"
        configuration.WriteTo.Console(theme=Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code, outputTemplate=t)

type Logging() =

    [<System.Runtime.CompilerServices.Extension>]
    static member Configure(configuration : LoggerConfiguration, ?verbose) =
        configuration
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
        configuration.WriteTo.Async(bufferSize = 65536, blockWhenFull = true, configure = System.Action<_> configure)

    [<System.Runtime.CompilerServices.Extension>]
    static member Sinks(configuration : LoggerConfiguration, configureMetricsSinks, verboseStore) =
        configuration.Sinks(configureMetricsSinks, Sinks.console, ?isMetric = if verboseStore then None else Some Log.isStoreMetrics)

/// A typical app will likely have health checks etc, implying the wireup would be via `UseMetrics()` and thus not use this ugly code directly
let startMetricsServer port : IDisposable =
    let metricsServer = new Prometheus.KestrelMetricServer(port = port)
    let ms = metricsServer.Start()
    Log.Information("Prometheus /metrics endpoint on port {port}", port)
    { new IDisposable with member x.Dispose() = ms.Stop(); (metricsServer :> IDisposable).Dispose() }

module Exception =

    let dump verboseStore (log : ILogger) (exn : exn) =
        match exn with
        | :? Microsoft.Azure.Cosmos.CosmosException as e
            when (e.StatusCode = System.Net.HttpStatusCode.TooManyRequests
                  || e.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable)
                 && not verboseStore -> ()
        | Equinox.DynamoStore.Exceptions.ProvisionedThroughputExceeded
        | :? TimeoutException when not verboseStore -> ()
        | _ ->
            log.Information(exn, "Unhandled")
