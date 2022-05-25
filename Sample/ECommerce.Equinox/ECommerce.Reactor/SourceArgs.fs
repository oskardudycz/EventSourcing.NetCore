/// Commandline arguments and/or secrets loading specifications
module ECommerce.Reactor.SourceArgs

open ECommerce.Infrastructure // Args
open Argu
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

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let discovery =                     a.TryGetResult Connection |> Option.defaultWith (fun () -> c.CosmosConnection) |> Equinox.CosmosStore.Discovery.ConnectionString
        let mode =                          a.TryGetResult ConnectionMode
        let timeout =                       a.GetResult(Timeout, 5.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(Retries, 9) //
        let maxRetryWaitTime =              a.GetResult(RetriesWaitTime, 30.) |> TimeSpan.FromSeconds
        let connector =                     Equinox.CosmosStore.CosmosStoreConnector(discovery, timeout, retries, maxRetryWaitTime, ?mode = mode)
        let database =                      a.TryGetResult Database  |> Option.defaultWith (fun () -> c.CosmosDatabase)
        let container =                     a.TryGetResult Container |> Option.defaultWith (fun () -> c.CosmosContainer)
        let leaseContainer =                a.GetResult(LeaseContainer, container + "-aux")
        let fromTail =                      a.Contains FromTail
        let maxItems =                      a.TryGetResult MaxItems
        let lagFrequency =                  a.GetResult(LagFreqM, 1.) |> TimeSpan.FromMinutes
        member val Verbose =                a.Contains Verbose
        member private _.ConnectLeases() =  connector.CreateUninitialized(database, leaseContainer)
        member x.ConnectStoreAndMonitored() = connector.ConnectStoreAndMonitored(database, container)
        member x.MonitoringParams(log : ILogger) =
            let leases : Microsoft.Azure.Cosmos.Container = x.ConnectLeases()
            log.Information("ChangeFeed Leases Database {db} Container {container}. MaxItems limited to {maxItems}",
                leases.Database.Id, leases.Id, Option.toNullable maxItems)
            if fromTail then log.Warning("(If new projector group) Skipping projection of all existing events.")
            (leases, fromTail, maxItems, lagFrequency)

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

        | [<AltCommandLine "-i">]           IndexTable of string
        | [<AltCommandLine "-mi"; Unique>]  MaxItems of int
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-d">]           StreamsDop of int
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | ServiceUrl _ ->           $"specify a server endpoint for a Dynamo account. (optional if environment variable {Args.SERVICE_URL} specified)"
                | AccessKey _ ->            $"specify an access key id for a Dynamo account. (optional if environment variable {Args.ACCESS_KEY} specified)"
                | SecretKey _ ->            $"specify a secret access key for a Dynamo account. (optional if environment variable {Args.SECRET_KEY} specified)"
                | Retries _ ->              "specify operation retries (default: 9)."
                | RetriesTimeoutS _ ->      "specify max wait-time including retries in seconds (default: 60)"
                | Table _ ->                $"specify a table name for the primary store. (optional if environment variable {Args.TABLE} specified)"

                | IndexTable _ ->           $"specify a table name for the index store. (optional if environment variable {Args.INDEX_TABLE} specified. default: `Table`+\"-index\")"
                | MaxItems _ ->             "maximum events to load in a batch. Default: 100"
                | FromTail _ ->             "(iff the Consumer Name is fresh) - force skip to present Position. Default: Never skip an event."
                | StreamsDop _ ->           "parallelism when loading events from Store Feed Source. Default 4"

    and Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let serviceUrl =                    a.TryGetResult ServiceUrl |> Option.defaultWith (fun () -> c.DynamoServiceUrl)
        let accessKey =                     a.TryGetResult AccessKey  |> Option.defaultWith (fun () -> c.DynamoAccessKey)
        let secretKey =                     a.TryGetResult SecretKey  |> Option.defaultWith (fun () -> c.DynamoSecretKey)
        let table =                         a.TryGetResult Table      |> Option.defaultWith (fun () -> c.DynamoTable)
        let indexTable =                    a.TryGetResult IndexTable |> Option.orElseWith  (fun () -> c.DynamoIndexTable)
                                                                      |> Option.defaultWith (fun () -> table + "-index")
        let maxItems =                      a.GetResult(MaxItems, 100)
        let fromTail =                      a.Contains FromTail
        let streamsDop =                    a.GetResult(StreamsDop, 4)
        let timeout =                       a.GetResult(RetriesTimeoutS, 60.) |> TimeSpan.FromSeconds
        let retries =                       a.GetResult(Retries, 9)
        let connector =                     Equinox.DynamoStore.DynamoStoreConnector(serviceUrl, accessKey, secretKey, timeout, retries)
        let client =                        connector.CreateClient()
        member val Verbose =                a.Contains Verbose
        member _.Connect() =                let mainClient = connector.ConnectStore(client, "Main", table)
                                            let streamsContext = Equinox.DynamoStore.DynamoStoreContext(mainClient)
                                            mainClient, streamsContext
        member _.MonitoringParams(log : ILogger) =
            log.Information("DynamoStoreSource MaxItems {maxItems} Hydrater parallelism {streamsDop}", maxItems, streamsDop)
            if fromTail then log.Warning("(If new projector group) Skipping projection of all existing events.")
            let indexClient = Equinox.DynamoStore.DynamoStoreClient(client, indexTable)
            indexClient.LogConfiguration("Index", log)
            indexClient, fromTail, maxItems, streamsDop

module Esdb =

    [<NoEquality; NoComparison>]
    type Parameters =
        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-c">]           Connection of string
        | [<AltCommandLine "-p"; Unique>]   Credentials of string
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int

// Not implemented in Propulsion.EventStoreDb yet (#good-first-issue)
//        | [<AltCommandLine "-Z"; Unique>]   FromTail

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<Args.Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Dynamo of ParseResults<Args.Dynamo.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | Connection _ ->           "EventStore Connection String. (optional if environment variable EQUINOX_ES_CONNECTION specified)"
                | Credentials _ ->          "Credentials string for EventStore (used as part of connection string, but NOT logged). Default: use EQUINOX_ES_CREDENTIALS environment variable (or assume no credentials)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."

//                | FromTail ->               "Start the processing from the Tail"

                | Cosmos _ ->               "CosmosDB Target Store parameters (also used for checkpoint storage)."
                | Dynamo _ ->               "DynamoDB Target Store parameters (also used for checkpoint storage)."

    [<RequireQualifiedAccess; NoComparison; NoEquality>]
    type TargetStoreArguments = Cosmos of Args.Cosmos.Arguments | Dynamo of Args.Dynamo.Arguments

    type Arguments(c : Configuration, a : ParseResults<Parameters>) =
        let        tailSleepInterval =      TimeSpan.FromSeconds 0.5
        let        maxItems =               100
        member val Verbose =                a.Contains Verbose
        member val ConnectionString =       a.TryGetResult(Connection) |> Option.defaultWith (fun () -> c.EventStoreConnection)
        member val Credentials =            a.TryGetResult(Credentials) |> Option.orElseWith (fun () -> c.EventStoreCredentials) |> Option.toObj
        member val Retries =                a.GetResult(Retries, 3)
        member val Timeout =                a.GetResult(Timeout, 20.) |> TimeSpan.FromSeconds

        member x.Connect(log: ILogger, appName, nodePreference) =
            let connection = x.ConnectionString
            log.Information("EventStore {discovery}", connection)
            let discovery = String.Join(";", connection, x.Credentials) |> Equinox.EventStoreDb.Discovery.ConnectionString
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Equinox.EventStoreDb.EventStoreConnector(x.Timeout, x.Retries, tags=tags)
                .Establish(appName, discovery, Equinox.EventStoreDb.ConnectionStrategy.ClusterSingle nodePreference)
        member _.MonitoringParams(log : ILogger) =
            log.Information("EventStoreSource MaxItems {maxItems}", maxItems)
            maxItems, tailSleepInterval

        (*  Propulsion.EventStoreDb does not implement a checkpoint storage mechanism,
            perhaps port https://github.com/absolutejam/Propulsion.EventStoreDB ?
            or fork/finish https://github.com/jet/dotnet-templates/pull/81
            alternately one could use a SQL Server DB via Propulsion.SqlStreamStore *)
        // NOTE as a result we borrow the target/read model store to host the checkpoints in the case of the Reactor app
        member val TargetStoreArgs : TargetStoreArguments =
            match a.GetSubCommand() with
            | Cosmos cosmos -> TargetStoreArguments.Cosmos (Args.Cosmos.Arguments (c, cosmos))
            | Dynamo dynamo -> TargetStoreArguments.Dynamo (Args.Dynamo.Arguments (c, dynamo))
            | _ -> Args.missingArg "Must specify `cosmos` or `dynamo` target store when source is `esdb`"

[<RequireQualifiedAccess; NoComparison; NoEquality>]
type Store = Cosmos of Cosmos.Arguments | Dynamo of Dynamo.Arguments | Esdb of Esdb.Arguments
let verboseRequested = function
    | Store.Cosmos a -> a.Verbose
    | Store.Dynamo a -> a.Verbose
    | Store.Esdb a -> a.Verbose
let dumpMetrics = function
    | Store.Cosmos _ -> Equinox.CosmosStore.Core.Log.InternalMetrics.dump
    | Store.Dynamo _ -> Equinox.DynamoStore.Core.Log.InternalMetrics.dump
    | Store.Esdb _ -> Equinox.EventStoreDb.Log.InternalMetrics.dump
