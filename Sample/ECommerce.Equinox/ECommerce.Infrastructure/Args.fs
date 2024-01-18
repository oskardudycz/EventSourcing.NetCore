/// Commandline arguments and/or secrets loading specifications
module Args

open System

let [<Literal>] REGION =                    "EQUINOX_DYNAMO_REGION"
let [<Literal>] SERVICE_URL =               "EQUINOX_DYNAMO_SERVICE_URL"
let [<Literal>] ACCESS_KEY =                "EQUINOX_DYNAMO_ACCESS_KEY_ID"
let [<Literal>] SECRET_KEY =                "EQUINOX_DYNAMO_SECRET_ACCESS_KEY"
let [<Literal>] TABLE =                     "EQUINOX_DYNAMO_TABLE"
let [<Literal>] INDEX_TABLE =               "EQUINOX_DYNAMO_TABLE_INDEX"

type Configuration(tryGet : string -> string option) =

    let get key =                           match tryGet key with Some value -> value | None -> failwith $"Missing Argument/Environment Variable %s{key}"
    member val tryGet =                     tryGet

    member _.CosmosConnection =             get "EQUINOX_COSMOS_CONNECTION"
    member _.CosmosDatabase =               get "EQUINOX_COSMOS_DATABASE"
    member _.CosmosContainer =              get "EQUINOX_COSMOS_CONTAINER"

    member _.DynamoServiceUrl =             get SERVICE_URL
    member _.DynamoAccessKey =              get ACCESS_KEY
    member _.DynamoSecretKey =              get SECRET_KEY
    member _.DynamoTable =                  get TABLE
    member _.DynamoRegion =                 tryGet REGION

    member _.EventStoreConnection =         get "EQUINOX_ES_CONNECTION"
    // member _.EventStoreCredentials =        get "EQUINOX_ES_CREDENTIALS"
    member _.MaybeEventStoreConnection =    tryGet "EQUINOX_ES_CONNECTION"
    member _.MaybeEventStoreCredentials =   tryGet "EQUINOX_ES_CREDENTIALS"

    member _.SqlStreamStoreConnection =     get "SQLSTREAMSTORE_CONNECTION"
    member _.SqlStreamStoreCredentials =    tryGet "SQLSTREAMSTORE_CREDENTIALS"
    member _.SqlStreamStoreCredentialsCheckpoints = tryGet "SQLSTREAMSTORE_CREDENTIALS_CHECKPOINTS"
    member _.SqlStreamStoreDatabase =       get "SQLSTREAMSTORE_DATABASE"
    member _.SqlStreamStoreContainer =      get "SQLSTREAMSTORE_CONTAINER"

    member x.PrometheusPort =               tryGet "PROMETHEUS_PORT" |> Option.map int

// Type used to represent where checkpoints (for either the FeedConsumer position, or for a Reactor's Event Store subscription position) will be stored
// In a typical app you don't have anything like this as you'll simply use your primary Event Store (see)
module Checkpoints =

    [<RequireQualifiedAccess; NoComparison; NoEquality>]
    type Config =
        | Cosmos of Equinox.CosmosStore.CosmosStoreContext * Equinox.Cache
        | Dynamo of Equinox.DynamoStore.DynamoStoreContext * Equinox.Cache
        (*  Propulsion.EventStoreDb does not implement a native checkpoint storage mechanism,
            perhaps port https://github.com/absolutejam/Propulsion.EventStoreDB ?
            or fork/finish https://github.com/jet/dotnet-templates/pull/81
            alternately one could use a SQL Server DB via Propulsion.SqlStreamStore

            For now, we store the Checkpoints in one of the above stores as this sample uses one for the read models anyway *)

    let create (consumerGroup, checkpointInterval) storeLog: Config -> Propulsion.Feed.IFeedCheckpointStore = function
        | Config.Cosmos (context, cache) ->
            Propulsion.Feed.ReaderCheckpoint.CosmosStore.create storeLog (consumerGroup, checkpointInterval) (context, cache)
        | Config.Dynamo (context, cache) ->
            Propulsion.Feed.ReaderCheckpoint.DynamoStore.create storeLog (consumerGroup, checkpointInterval) (context, cache)
    let createCheckpointStore (group, checkpointInterval, store) : Propulsion.Feed.IFeedCheckpointStore =
        let checkpointStore =
            match store with
            | Store.Config.Cosmos (context, cache) -> Config.Cosmos (context, cache)
            | Store.Config.Dynamo (context, cache) -> Config.Dynamo (context, cache)
            | Store.Config.Esdb _
            | Store.Config.Memory _
            | Store.Config.Sss _ -> failwith "unexpected"
        create (group, checkpointInterval) Store.Metrics.log checkpointStore

open Argu

module Cosmos =

    type [<NoEquality; NoComparison>] Parameters =
        | [<AltCommandLine "-V"; Unique>]   Verbose
        | [<AltCommandLine "-m">]           ConnectionMode of Microsoft.Azure.Cosmos.ConnectionMode
        | [<AltCommandLine "-s">]           Connection of string
        | [<AltCommandLine "-d">]           Database of string
        | [<AltCommandLine "-c">]           Container of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesWaitTime of float
        interface IArgParserTemplate with
            member p.Usage = p |> function
                | Verbose _ ->              "request verbose logging."
                | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                | Database _ ->             "specify a database name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                | Container _ ->            "specify a container name for Cosmos store. (optional if environment variable EQUINOX_COSMOS_CONTAINER specified)"
                | Timeout _ ->              "specify operation timeout in seconds (default: 5)."
                | Retries _ ->              "specify operation retries (default: 1)."
                | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds (default: 5)"
    type Arguments(c : Configuration, p : ParseResults<Parameters>) =
        let connection =                    p.GetResult(Connection, fun () -> c.CosmosConnection)
        let discovery =                     Equinox.CosmosStore.Discovery.ConnectionString connection
        let mode =                          p.TryGetResult ConnectionMode
        let timeout =                       p.GetResult(Timeout, 5.) |> TimeSpan.FromSeconds
        let retries =                       p.GetResult(Retries, 1)
        let maxRetryWaitTime =              p.GetResult(RetriesWaitTime, 5.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
        let database =                      p.GetResult(Database, fun () -> c.CosmosDatabase)
        let container =                     p.GetResult(Container, fun () -> c.CosmosContainer)
        member val Verbose =                p.Contains Verbose
        member _.Connect() =                connector.Connect("Target", database, container)

module Dynamo =

    type [<NoEquality; NoComparison>] Parameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-sr">]          RegionProfile of string
        | [<AltCommandLine "-su">]          ServiceUrl of string
        | [<AltCommandLine "-sa">]          AccessKey of string
        | [<AltCommandLine "-ss">]          SecretKey of string
        | [<AltCommandLine "-t">]           Table of string
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesTimeoutS of float
        interface IArgParserTemplate with
            member p.Usage = p |> function
                | Verbose ->                "Include low level Store logging."
                | RegionProfile _ ->        "specify an AWS Region (aka System Name, e.g. \"us-east-1\") to connect to using the implicit AWS SDK/tooling config and/or environment variables etc. Optional if:\n" +
                                            "1) $" + REGION + " specified OR\n" +
                                            "2) Explicit `ServiceUrl`/$" + SERVICE_URL + "+`AccessKey`/$" + ACCESS_KEY + "+`Secret Key`/$" + SECRET_KEY + " specified.\n" +
                                            "See https://docs.aws.amazon.com/cli/latest/userguide/cli-configure-envvars.html for details"
                | ServiceUrl _ ->           "specify a server endpoint for a Dynamo account. (Not applicable if `ServiceRegion`/$" + REGION + " specified; Optional if $" + SERVICE_URL + " specified)"
                | AccessKey _ ->            "specify an access key id for a Dynamo account. (Not applicable if `ServiceRegion`/$" + REGION + " specified; Optional if $" + ACCESS_KEY + " specified)"
                | SecretKey _ ->            "specify a secret access key for a Dynamo account. (Not applicable if `ServiceRegion`/$" + REGION + " specified; Optional if $" + SECRET_KEY + " specified)"
                | Table _ ->                "specify a table name for the primary store. (optional if $" + TABLE + " specified)"
                | Retries _ ->              "specify operation retries (default: 1)."
                | RetriesTimeoutS _ ->      "specify max wait-time including retries in seconds (default: 5)"
    type Arguments(c : Configuration, p : ParseResults<Parameters>) =
        let conn =                          match p.TryGetResult RegionProfile |> Option.orElseWith (fun () -> c.DynamoRegion) with
                                            | Some systemName ->
                                                Choice1Of2 systemName
                                            | None ->
                                                let serviceUrl =  p.TryGetResult ServiceUrl |> Option.defaultWith (fun () -> c.DynamoServiceUrl)
                                                let accessKey =   p.TryGetResult AccessKey  |> Option.defaultWith (fun () -> c.DynamoAccessKey)
                                                let secretKey =   p.TryGetResult SecretKey  |> Option.defaultWith (fun () -> c.DynamoSecretKey)
                                                Choice2Of2 (serviceUrl, accessKey, secretKey)
        let retries =                       p.GetResult(Retries, 1)
        let timeout =                       p.GetResult(RetriesTimeoutS, 5.) |> TimeSpan.FromSeconds
        let connector =                     match conn with
                                            | Choice1Of2 systemName ->
                                                Equinox.DynamoStore.DynamoStoreConnector(systemName, timeout, retries)
                                            | Choice2Of2 (serviceUrl, accessKey, secretKey) ->
                                                Equinox.DynamoStore.DynamoStoreConnector(serviceUrl, accessKey, secretKey, timeout, retries)
        let table =                         p.TryGetResult Table      |> Option.defaultWith (fun () -> c.DynamoTable)
        member val Verbose =                p.Contains Verbose
        member _.Connect() =                connector.CreateClient().CreateContext("Main", table)

module Esdb =

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-c">]           Connection of string
        | [<AltCommandLine "-p"; Unique>]   Credentials of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Dynamo of ParseResults<Dynamo.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | Connection _ ->           "EventStore Connection String. (optional if environment variable EQUINOX_ES_CONNECTION specified)"
                | Credentials _ ->          "Credentials string for EventStore (used as part of connection string, but NOT logged). Default: use EQUINOX_ES_CREDENTIALS environment variable (or assume no credentials)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."

                // Feed Consumer app needs somewhere to store checkpoints
                // Here we align with the structure of the commandline parameters for the Reactor app and also require a Dynamo or Cosmos instance to be specified
                | Cosmos _ ->               "CosmosDB (Checkpoint/Target) Store parameters (Not applicable for Web app)."
                | Dynamo _ ->               "DynamoDB (Checkpoint/Target) Store parameters (Not applicable to Web app)."

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let connectionStringLoggable =      a.TryGetResult Connection |> Option.defaultWith (fun () -> c.EventStoreConnection)
        let credentials =                   a.TryGetResult Credentials |> Option.orElseWith (fun () -> c.MaybeEventStoreCredentials)
        let retries =                       a.GetResult(Retries, 3)
        let timeout =                       a.GetResult(Timeout, 20.) |> TimeSpan.FromSeconds
        member _.Verbose =                  a.Contains Verbose

        member x.Connect(log : Serilog.ILogger, appName, nodePreference) : Equinox.EventStoreDb.EventStoreConnection =
            log.Information("EventStore {discovery}", connectionStringLoggable)
            let discovery = match credentials with Some x -> String.Join(";", connectionStringLoggable, x) | None -> connectionStringLoggable
                            |> Equinox.EventStoreDb.Discovery.ConnectionString
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Equinox.EventStoreDb.EventStoreConnector(timeout, retries, tags = tags)
                .Establish(appName, discovery, Equinox.EventStoreDb.ConnectionStrategy.ClusterSingle nodePreference)

        member _.SecondaryStoreArgs : SecondaryStoreArgs =
            match a.GetSubCommand() with
            | Cosmos cosmos -> SecondaryStoreArgs.Cosmos (Cosmos.Arguments(c, cosmos))
            | Dynamo dynamo -> SecondaryStoreArgs.Dynamo (Dynamo.Arguments(c, dynamo))
            | _ -> a.Raise "Must specify `cosmos` or `dynamo` target store when source is `esdb`"

        member x.ConnectCheckpointStore(cache) =
            match x.SecondaryStoreArgs with
            | SecondaryStoreArgs.Cosmos a ->
                let context = a.Connect() |> Async.RunSynchronously
                Checkpoints.Config.Cosmos (context, cache)
            | SecondaryStoreArgs.Dynamo a ->
                let context = a.Connect()
                Checkpoints.Config.Dynamo (context, cache)

    and [<RequireQualifiedAccess; NoComparison; NoEquality>]
        SecondaryStoreArgs =
        | Cosmos of Cosmos.Arguments
        | Dynamo of Dynamo.Arguments

module Sss =

    // TOCONSIDER: add DB connectors other than MsSql
    type [<NoEquality; NoComparison>] Parameters =
        | [<AltCommandLine "-c"; Unique>]   Connection of string
        | [<AltCommandLine "-p"; Unique>]   Credentials of string
        | [<AltCommandLine "-s">]           Schema of string
        | [<AltCommandLine "-cc"; Unique>]  CheckpointsConnection of string
        | [<AltCommandLine "-cp"; Unique>]  CheckpointsCredentials of string
        // | [<AltCommandLine "-b"; Unique>]   BatchSize of int
        interface IArgParserTemplate with
            member p.Usage = p |> function
                | Connection _ ->           "Connection string for SqlStreamStore db. Optional if SQLSTREAMSTORE_CONNECTION specified"
                | Credentials _ ->          "Credentials string for SqlStreamStore db (used as part of connection string, but NOT logged). Default: use SQLSTREAMSTORE_CREDENTIALS environment variable (or assume no credentials)"
                | Schema _ ->               "Database schema name"
                | CheckpointsConnection _ ->"Connection string for Checkpoints sql db. Optional if SQLSTREAMSTORE_CONNECTION_CHECKPOINTS specified. Default: same as `Connection`"
                | CheckpointsCredentials _ ->"Credentials string for Checkpoints sql db. (used as part of checkpoints connection string, but NOT logged). Default (when no `CheckpointsConnection`: use `Credentials. Default (when `CheckpointsConnection` specified): use SQLSTREAMSTORE_CREDENTIALS_CHECKPOINTS environment variable (or assume no credentials)"
                // | BatchSize _ ->            "Maximum events to request from feed. Default: 512"

    type Arguments(c : Configuration, p : ParseResults<Parameters>) =
        // let batchSize =                     p.GetResult(BatchSize, 512)
        let connection =                    p.TryGetResult Connection |> Option.defaultWith (fun () -> c.SqlStreamStoreConnection)
        let credentials =                   p.TryGetResult Credentials |> Option.orElseWith (fun () -> c.SqlStreamStoreCredentials) |> Option.toObj
        let schema =                        p.GetResult(Schema, null)

        member x.Connect() =
            let conn, creds, schema, autoCreate = connection, credentials, schema, false
            let sssConnectionString = String.Join(";", conn, creds)
            Serilog.Log.Information("SqlStreamStore MsSql Connection {connectionString} Schema {schema} AutoCreate {autoCreate}", conn, schema, autoCreate)
            let rawStore = Equinox.SqlStreamStore.MsSql.Connector(sssConnectionString, schema, autoCreate=autoCreate).Connect() |> Async.RunSynchronously
            Equinox.SqlStreamStore.SqlStreamStoreConnection(rawStore)
        member x.BuildCheckpointsConnectionString() =
            let c, cs =
                match p.TryGetResult CheckpointsConnection, p.TryGetResult CheckpointsCredentials with
                | Some c, Some p -> c, String.Join(";", c, p)
                | None, Some p ->   let c = connection in c, String.Join(";", c, p)
                | None, None ->     let c = connection in c, String.Join(";", c, credentials)
                | Some cc, None ->  let p = c.SqlStreamStoreCredentialsCheckpoints |> Option.toObj
                                    cc, String.Join(";", cc, p)
            Serilog.Log.Information("Checkpoints MsSql Connection {connectionString}", c)
            cs
        member x.CreateCheckpointStoreSql(groupName) : Propulsion.Feed.IFeedCheckpointStore =
            let connectionString = x.BuildCheckpointsConnectionString()
            Propulsion.SqlStreamStore.ReaderCheckpoint.Service(connectionString, groupName)

type [<RequireQualifiedAccess; NoComparison; NoEquality>]
    StoreArgs =
    | Cosmos of Cosmos.Arguments
    | Dynamo of Dynamo.Arguments
    | Esdb of Esdb.Arguments
    | Sss of Sss.Arguments

module StoreArgs =

    let connectTarget targetStore cache =
        match targetStore with
        | StoreArgs.Cosmos a ->
            let context = a.Connect() |> Async.RunSynchronously
            Store.Config.Cosmos (context, cache)
        | StoreArgs.Dynamo a ->
            let context = a.Connect()
            Store.Config.Dynamo (context, cache)
        | StoreArgs.Esdb a ->
            let context = a.Connect(Serilog.Log.Logger, "Main", EventStore.Client.NodePreference.Leader) |> EventStoreContext.create
            Store.Config.Esdb (context, cache)
        | StoreArgs.Sss a ->
            let context = a.Connect() |> SqlStreamStoreContext.create
            Store.Config.Sss (context, cache)
    let verboseRequested = function
        | StoreArgs.Cosmos a -> a.Verbose
        | StoreArgs.Dynamo a -> a.Verbose
        | StoreArgs.Esdb a -> a.Verbose
        | StoreArgs.Sss a -> false
    let dumpMetrics = function
        | StoreArgs.Cosmos _ -> Equinox.CosmosStore.Core.Log.InternalMetrics.dump
        | StoreArgs.Dynamo _ -> Equinox.DynamoStore.Core.Log.InternalMetrics.dump
        | StoreArgs.Esdb _ ->   Equinox.EventStoreDb.Log.InternalMetrics.dump
        | StoreArgs.Sss _ ->    Equinox.SqlStreamStore.Log.InternalMetrics.dump
