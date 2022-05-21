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

type Equinox.DynamoStore.DynamoStoreClient with

    member x.LogConfiguration(role : string, ?log) =
        (defaultArg log Log.Logger).Information("DynamoStore {role:l} Table {table}", role) // TODO next ver has: , x.TableName)

type Equinox.DynamoStore.DynamoStoreConnector with

    member x.LogConfiguration() =
        Log.Information("DynamoStore {endpoint} Timeout {timeoutS}s Retries {retries}",
                        x.Endpoint, (let t = x.Timeout in t.TotalSeconds), x.Retries)

    member x.ConnectStore(client, role, table) =
        x.LogConfiguration()
        let storeClient = Equinox.DynamoStore.DynamoStoreClient(client, table)
        storeClient.LogConfiguration(role)
        storeClient

module DynamoStoreContext =

    /// Create with default packing and querying policies. Search for other `module DynamoStoreContext` impls for custom variations
    let create (storeClient : Equinox.DynamoStore.DynamoStoreClient) =
        Equinox.DynamoStore.DynamoStoreContext(storeClient, queryMaxItems = 100)

module EventStoreContext =

    let create (storeClient : Equinox.EventStoreDb.EventStoreConnection) =
        let batchingPolicy = Equinox.EventStoreDb.BatchingPolicy(maxBatchSize = 200)
        Equinox.EventStoreDb.EventStoreContext(storeClient, batchingPolicy)

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
          |> fun l -> l.WriteTo.Sink(Propulsion.Prometheus.LogSink(tags))

    let equinoxAndPropulsionReactorMetrics tags (l : LoggerConfiguration) =
        l |> equinoxAndPropulsionMetrics tags
          |> fun l -> l.WriteTo.Sink(Propulsion.CosmosStore.Prometheus.LogSink(tags))
                       .WriteTo.Sink(Propulsion.Feed.Prometheus.LogSink(tags)) // Esdb and Dynamo indirectly provide metrics via Feed

    let equinoxAndPropulsionFeedConsumerMetrics tags (l : LoggerConfiguration) =
        l |> equinoxAndPropulsionMetrics tags
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

module Args =

    exception MissingArg of message : string with override this.Message = this.message
    let missingArg msg = raise (MissingArg msg)

    let [<Literal>] SERVICE_URL =               "EQUINOX_DYNAMO_SERVICE_URL"
    let [<Literal>] ACCESS_KEY =                "EQUINOX_DYNAMO_ACCESS_KEY_ID"
    let [<Literal>] SECRET_KEY =                "EQUINOX_DYNAMO_SECRET_ACCESS_KEY"
    let [<Literal>] TABLE =                     "EQUINOX_DYNAMO_TABLE"
    let [<Literal>] INDEX_TABLE =               "EQUINOX_DYNAMO_TABLE_INDEX"

    type Configuration(tryGet : string -> string option) =

        member val tryGet =                     tryGet
        member _.get key =                      match tryGet key with Some value -> value | None -> missingArg $"Missing Argument/Environment Variable %s{key}"

        member x.CosmosConnection =             x.get "EQUINOX_COSMOS_CONNECTION"
        member x.CosmosDatabase =               x.get "EQUINOX_COSMOS_DATABASE"
        member x.CosmosContainer =              x.get "EQUINOX_COSMOS_CONTAINER"

        member x.DynamoServiceUrl =             x.get SERVICE_URL
        member x.DynamoAccessKey =              x.get ACCESS_KEY
        member x.DynamoSecretKey =              x.get SECRET_KEY
        member x.DynamoTable =                  x.get TABLE

        member x.EventStoreConnection =         x.get "EQUINOX_ES_CONNECTION"
        member _.EventStoreCredentials =        tryGet "EQUINOX_ES_CREDENTIALS"

        member x.PrometheusPort =               tryGet "PROMETHEUS_PORT" |> Option.map int

    open Argu

    module Cosmos =

        [<NoEquality; NoComparison>]
        type Parameters =
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
                    | Verbose _ ->              "request verbose logging."
                    | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                    | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                    | Database _ ->             "specify a database name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                    | Container _ ->            "specify a container name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_CONTAINER specified)"
                    | Timeout _ ->              "specify operation timeout in seconds (default: 5)."
                    | Retries _ ->              "specify operation retries (default: 1)."
                    | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds (default: 5)"

        type Arguments(c : Configuration, a : ParseResults<Parameters>) =
            let discovery =                    a.TryGetResult Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
            let mode =                          a.TryGetResult ConnectionMode
            let timeout =                       a.GetResult(Timeout, 5.) |> TimeSpan.FromSeconds
            let retries =                       a.GetResult(Retries, 1)
            let maxRetryWaitTime =              a.GetResult(RetriesWaitTime, 5.) |> TimeSpan.FromSeconds
            let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
            let database =                      a.TryGetResult Database |> Option.defaultWith (fun () -> c.CosmosDatabase)
            let container =                     a.TryGetResult Container |> Option.defaultWith (fun () -> c.CosmosContainer)
            member val Verbose =                a.Contains Verbose
            member _.Connect() =                connector.ConnectStore("Main", database, container)

    module Dynamo =

        [<NoEquality; NoComparison>]
        type Parameters =
            | [<AltCommandLine "-V">]           Verbose
            | [<AltCommandLine "-s">]           ServiceUrl of string
            | [<AltCommandLine "-sa">]          AccessKey of string
            | [<AltCommandLine "-ss">]          SecretKey of string
            | [<AltCommandLine "-t">]           Table of string
            | [<AltCommandLine "-r">]           Retries of int
            | [<AltCommandLine "-rt">]          RetriesTimeoutS of float
            interface IArgParserTemplate with
                member a.Usage = a |> function
                    | Verbose ->                "Include low level Store logging."
                    | ServiceUrl _ ->           "specify a server endpoint for a Dynamo account. (optional if environment variable " + SERVICE_URL + " specified)"
                    | AccessKey _ ->            "specify an access key id for a Dynamo account. (optional if environment variable " + ACCESS_KEY + " specified)"
                    | SecretKey _ ->            "specify a secret access key for a Dynamo account. (optional if environment variable " + SECRET_KEY + " specified)"
                    | Table _ ->                "specify a table name for the primary store. (optional if environment variable " + TABLE + " specified)"
                    | Retries _ ->              "specify operation retries (default: 1)."
                    | RetriesTimeoutS _ ->      "specify max wait-time including retries in seconds (default: 5)"

        type Arguments(c : Configuration, a : ParseResults<Parameters>) =
            let serviceUrl =                    a.TryGetResult ServiceUrl |> Option.defaultWith (fun () -> c.DynamoServiceUrl)
            let accessKey =                     a.TryGetResult AccessKey  |> Option.defaultWith (fun () -> c.DynamoAccessKey)
            let secretKey =                     a.TryGetResult SecretKey  |> Option.defaultWith (fun () -> c.DynamoSecretKey)
            let table =                         a.TryGetResult Table      |> Option.defaultWith (fun () -> c.DynamoTable)
            let retries =                       a.GetResult(Retries, 1)
            let timeout =                       a.GetResult(RetriesTimeoutS, 5.) |> TimeSpan.FromSeconds
            let connector =                     Equinox.DynamoStore.DynamoStoreConnector(serviceUrl, accessKey, secretKey, retries, timeout)
            member val Verbose =                a.Contains Verbose
            member _.Connect() =                connector.ConnectStore("Main", table)

    module Esdb =

        [<NoEquality; NoComparison>]
        type Parameters =
            | [<AltCommandLine "-V">]           Verbose
            | [<AltCommandLine "-c">]           Connection of string
            | [<AltCommandLine "-p"; Unique>]   Credentials of string
            | [<AltCommandLine "-o">]           Timeout of float
            | [<AltCommandLine "-r">]           Retries of int
    //        | [<AltCommandLine "-oh">]          HeartbeatTimeout of float
            interface IArgParserTemplate with
                member a.Usage = a |> function
                    | Verbose ->                "Include low level Store logging."
                    | Connection _ ->           "EventStore Connection String. (optional if environment variable EQUINOX_ES_CONNECTION specified)"
                    | Credentials _ ->          "Credentials string for EventStore (used as part of connection string, but NOT logged). Default: use EQUINOX_ES_CREDENTIALS environment variable (or assume no credentials)"
                    | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                    | Retries _ ->              "specify operation retries. Default: 3."

        type Arguments(c : Configuration, a : ParseResults<Parameters>) =
            member val ConnectionString =       a.TryGetResult(Connection) |> Option.defaultWith (fun () -> c.EventStoreConnection)
            member val Credentials =            a.TryGetResult(Credentials) |> Option.orElseWith (fun () -> c.EventStoreCredentials) |> Option.toObj
            member val Retries =                a.GetResult(Retries, 3)
            member val Timeout =                a.GetResult(Timeout, 20.) |> TimeSpan.FromSeconds
            member _.Verbose =                  a.Contains Verbose
            member x.Connect(log: ILogger, appName, nodePreference) =
                let connection = x.ConnectionString
                log.Information("EventStore {discovery}", connection)
                let discovery = String.Join(";", connection, x.Credentials) |> Equinox.EventStoreDb.Discovery.ConnectionString
                let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
                Equinox.EventStoreDb.EventStoreConnector(x.Timeout, x.Retries, tags = tags)
                    .Establish(appName, discovery, Equinox.EventStoreDb.ConnectionStrategy.ClusterSingle nodePreference)

module Exception =

    let dump verboseStore (log : ILogger) (exn : exn) =
        match exn with
        | :? Microsoft.Azure.Cosmos.CosmosException as e
            when (e.StatusCode = System.Net.HttpStatusCode.TooManyRequests
                  || e.StatusCode = System.Net.HttpStatusCode.ServiceUnavailable)
                 && not verboseStore -> ()
        | Equinox.DynamoStore.Exceptions.ProvisionedThroughputExceeded when not verboseStore -> ()
        | _ ->
            log.Information(exn, "Unhandled")
