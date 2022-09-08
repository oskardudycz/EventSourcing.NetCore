module ECommerce.Reactor.SourceArgs

open Argu
open ECommerce.Domain // Config etc
open ECommerce.Infrastructure // Args etc
open Serilog
open System

type Configuration(tryGet) =
    inherit Args.Configuration(tryGet)
    member _.DynamoIndexTable =             tryGet Args.INDEX_TABLE

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

        | [<AltCommandLine "-a"; Unique>]   LeaseContainer of string
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-mi"; Unique>]  MaxItems of int
        | [<AltCommandLine "-l"; Unique>]   LagFreqM of float
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "request Verbose Logging from ChangeFeedProcessor and Store. Default: off"
                | ConnectionMode _ ->       "override the connection mode. Default: Direct."
                | Connection _ ->           "specify a connection string for a Cosmos account. (optional if environment variable EQUINOX_COSMOS_CONNECTION specified)"
                | Database _ ->             "specify a database name for store. (optional if environment variable EQUINOX_COSMOS_DATABASE specified)"
                | Container _ ->            "specify a container name for store. (optional if environment variable EQUINOX_COSMOS_CONTAINER specified)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 5."
                | Retries _ ->              "specify operation retries. Default: 9."
                | RetriesWaitTime _ ->      "specify max wait-time for retry when being throttled by Cosmos in seconds. Default: 30."

                | LeaseContainer _ ->       "specify Container Name (in this [target] Database) for Leases container. Default: `SourceContainer` + `-aux`."
                | FromTail _ ->             "(iff the Consumer Name is fresh) - force skip to present Position. Default: Never skip an event."
                | MaxItems _ ->             "maximum item count to supply for the Change Feed query. Default: use response size limit"
                | LagFreqM _ ->             "specify frequency (minutes) to dump lag stats. Default: 1"

    type Arguments(c : Args.Configuration, a : ParseResults<Parameters>) =
        let discovery =                     a.TryGetResult Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
        let mode =                          a.TryGetResult ConnectionMode
        let timeout =                       a.GetResult(Timeout, 5.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(Retries, 9)
        let maxRetryWaitTime =              a.GetResult(RetriesWaitTime, 30.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
        let database =                      a.TryGetResult Database  |> Option.defaultWith (fun () -> c.CosmosDatabase)
        let containerId =                   a.TryGetResult Container |> Option.defaultWith (fun () -> c.CosmosContainer)
        let leaseContainerId =              a.GetResult(LeaseContainer, containerId + "-aux")
        let fromTail =                      a.Contains FromTail
        let maxItems =                      a.TryGetResult MaxItems
        let tailSleepInterval =             TimeSpan.FromMilliseconds 500.
        let lagFrequency =                  a.GetResult(LagFreqM, 1.) |> TimeSpan.FromMinutes
        member _.Verbose =                  a.Contains Verbose
        member private _.ConnectLeases() =  connector.CreateUninitialized(database, leaseContainerId)
        member x.MonitoringParams(log : ILogger) =
            let leases : Microsoft.Azure.Cosmos.Container = x.ConnectLeases()
            log.Information("ChangeFeed Leases Database {db} Container {container}. MaxItems limited to {maxItems}",
                leases.Database.Id, leases.Id, Option.toNullable maxItems)
            if fromTail then log.Warning("(If new projector group) Skipping projection of all existing events.")
            (leases, fromTail, maxItems, tailSleepInterval, lagFrequency)
        member x.ConnectStoreAndMonitored() = connector.ConnectStoreAndMonitored(database, containerId)

module Dynamo =

    type [<NoEquality; NoComparison>] Parameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-s">]           ServiceUrl of string
        | [<AltCommandLine "-sa">]          AccessKey of string
        | [<AltCommandLine "-ss">]          SecretKey of string
        | [<AltCommandLine "-t">]           Table of string
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-rt">]          RetriesTimeoutS of float
        | [<AltCommandLine "-i">]           IndexTable of string
        | [<AltCommandLine "-is">]          IndexSuffix of string
        | [<AltCommandLine "-mi"; Unique>]  MaxItems of int
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-d">]           StreamsDop of int
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | ServiceUrl _ ->           "specify a server endpoint for a Dynamo account. (optional if environment variable " + Args.SERVICE_URL + " specified)"
                | AccessKey _ ->            "specify an access key id for a Dynamo account. (optional if environment variable " + Args.ACCESS_KEY + " specified)"
                | SecretKey _ ->            "specify a secret access key for a Dynamo account. (optional if environment variable " + Args.SECRET_KEY + " specified)"
                | Retries _ ->              "specify operation retries (default: 9)."
                | RetriesTimeoutS _ ->      "specify max wait-time including retries in seconds (default: 60)"
                | Table _ ->                "specify a table name for the primary store. (optional if environment variable " + Args.TABLE + " specified)"
                | IndexTable _ ->           "specify a table name for the index store. (optional if environment variable " + Args.INDEX_TABLE + " specified. default: `Table`+`IndexSuffix`)"
                | IndexSuffix _ ->          "specify a suffix for the index store. (optional if environment variable " + Args.INDEX_TABLE + " specified. default: \"-index\")"
                | MaxItems _ ->             "maximum events to load in a batch. Default: 100"
                | FromTail _ ->             "(iff the Consumer Name is fresh) - force skip to present Position. Default: Never skip an event."
                | StreamsDop _ ->           "parallelism when loading events from Store Feed Source. Default 4"

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let serviceUrl =                    a.TryGetResult ServiceUrl |> Option.defaultWith (fun () -> c.DynamoServiceUrl)
        let accessKey =                     a.TryGetResult AccessKey  |> Option.defaultWith (fun () -> c.DynamoAccessKey)
        let secretKey =                     a.TryGetResult SecretKey  |> Option.defaultWith (fun () -> c.DynamoSecretKey)
        let table =                         a.TryGetResult Table      |> Option.defaultWith (fun () -> c.DynamoTable)
        let indexSuffix =                   a.GetResult(IndexSuffix, "-index")
        let indexTable =                    a.TryGetResult IndexTable |> Option.orElseWith  (fun () -> c.DynamoIndexTable) |> Option.defaultWith (fun () -> table + indexSuffix)
        let fromTail =                      a.Contains FromTail
        let tailSleepInterval =             TimeSpan.FromMilliseconds 500.
        let maxItems =                      a.GetResult(MaxItems, 100)
        let streamsDop =                    a.GetResult(StreamsDop, 4)
        let timeout =                       a.GetResult(RetriesTimeoutS, 60.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(Retries, 9)
        let connector =                     Equinox.DynamoStore.DynamoStoreConnector(serviceUrl, accessKey, secretKey, timeout, retries)
        let client =                        connector.CreateClient()
        let indexStoreClient =              lazy client.ConnectStore("Index", indexTable)
        member val Verbose =                a.Contains Verbose
        member _.Connect() =                connector.LogConfiguration()
                                            client.ConnectStore("Main", table) |> DynamoStoreContext.create
        member _.MonitoringParams(log : ILogger) =
            log.Information("DynamoStoreSource MaxItems {maxItems} Hydrater parallelism {streamsDop}", maxItems, streamsDop)
            let indexStoreClient = indexStoreClient.Value
            if fromTail then log.Warning("(If new projector group) Skipping projection of all existing events.")
            indexStoreClient, fromTail, maxItems, tailSleepInterval, streamsDop
        member _.CreateCheckpointStore(group, cache) =
            let indexTable = indexStoreClient.Value
            indexTable.CreateCheckpointService(group, cache, Config.log)

module Esdb =

    type [<NoEquality; NoComparison>] Parameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-c">]           Connection of string
        | [<AltCommandLine "-p"; Unique>]   Credentials of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int

        | [<AltCommandLine "-mi"; Unique>]  MaxItems of int
        | [<AltCommandLine "-Z"; Unique>]   FromTail

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<Args.Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Dynamo of ParseResults<Args.Dynamo.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | Connection _ ->           "EventStore Connection String. (optional if environment variable EQUINOX_ES_CONNECTION specified)"
                | Credentials _ ->          "Credentials string for EventStore (used as part of connection string, but NOT logged). Default: use EQUINOX_ES_CREDENTIALS environment variable (or assume no credentials)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."

                | FromTail ->               "Start the processing from the Tail"
                | MaxItems _ ->             "maximum events to load in a batch. Default: 100"

                | Cosmos _ ->               "CosmosDB Target Store parameters (also used for checkpoint storage)."
                | Dynamo _ ->               "DynamoDB Target Store parameters (also used for checkpoint storage)."

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let connectionStringLoggable =      a.TryGetResult Connection |> Option.defaultWith (fun () -> c.EventStoreConnection)
        let credentials =                   a.TryGetResult Credentials |> Option.orElseWith (fun () -> c.EventStoreCredentials)
        let discovery =                     match credentials with Some x -> String.Join(";", connectionStringLoggable, x) | None -> connectionStringLoggable
                                            |> Equinox.EventStoreDb.Discovery.ConnectionString
        let startFromTail =                 a.Contains FromTail
        let maxItems =                      a.GetResult(MaxItems, 100)
        let tailSleepInterval =             TimeSpan.FromSeconds 0.5
        let retries =                       a.GetResult(Retries, 3)
        let timeout =                       a.GetResult(Timeout, 20.) |> TimeSpan.FromSeconds
        let checkpointInterval =            TimeSpan.FromHours 1.
        member val Verbose =                a.Contains Verbose

        member _.Connect(log : ILogger, appName, nodePreference) : Equinox.EventStoreDb.EventStoreConnection =
            log.Information("EventStore {discovery}", connectionStringLoggable)
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Equinox.EventStoreDb.EventStoreConnector(timeout, retries, tags = tags)
                .Establish(appName, discovery, Equinox.EventStoreDb.ConnectionStrategy.ClusterSingle nodePreference)

        member private _.TargetStoreArgs : Args.Esdb.TargetStoreArgs =
            match a.GetSubCommand() with
            | Cosmos cosmos -> Args.Esdb.TargetStoreArgs.Cosmos (Args.Cosmos.Arguments(c, cosmos))
            | Dynamo dynamo -> Args.Esdb.TargetStoreArgs.Dynamo (Args.Dynamo.Arguments(c, dynamo))
            | _ -> Args.missingArg "Must specify `cosmos` or `dynamo` target store when source is `esdb`"

        member _.MonitoringParams(log : ILogger) =
            log.Information("EventStoreSource MaxItems {maxItems} ", maxItems)
            startFromTail, maxItems, tailSleepInterval
        member x.ConnectTarget(cache) : Config.Store<_> =
            match x.TargetStoreArgs with
            | EsdbTargetStore.Cosmos a ->
                let context = a.Connect() |> Async.RunSynchronously |> CosmosStoreContext.create
                Config.Store.Cosmos (context, cache)
            | EsdbTargetStore.Dynamo a ->
                let context = a.Connect() |> DynamoStoreContext.create
                Config.Store.Dynamo (context, cache)
        member _.CreateCheckpointStore(group, store : Config.Store<_>) : Propulsion.Feed.IFeedCheckpointStore =
            let checkpointStore =
                match store with
                | Config.Store.Cosmos (context, cache) -> Args.CheckpointStore.Config.Cosmos (context, cache)
                | Config.Store.Dynamo (context, cache) -> Args.CheckpointStore.Config.Dynamo (context, cache)
                | Config.Store.Memory _ | Config.Store.Esdb _ -> failwith "unexpected"
            Args.CheckpointStore.create (group, checkpointInterval) Config.log checkpointStore
    and  EsdbTargetStore = Args.Esdb.TargetStoreArgs
