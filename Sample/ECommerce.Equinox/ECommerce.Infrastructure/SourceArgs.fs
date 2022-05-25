module ECommerce.Infrastructure.SourceArgs

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

        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Cosmos of ParseResults<Args.Cosmos.Parameters>
        | [<CliPrefix(CliPrefix.None); Unique(*ExactlyOnce is not supported*); Last>] Dynamo of ParseResults<Args.Dynamo.Parameters>
        interface IArgParserTemplate with
            member a.Usage = a |> function
                | Verbose ->                "Include low level Store logging."
                | Connection _ ->           "EventStore Connection String. (optional if environment variable EQUINOX_ES_CONNECTION specified)"
                | Credentials _ ->          "Credentials string for EventStore (used as part of connection string, but NOT logged). Default: use EQUINOX_ES_CREDENTIALS environment variable (or assume no credentials)"
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."

                | Cosmos _ ->               "CosmosDB (Checkpoint) Store parameters."
                | Dynamo _ ->               "DynamoDB (Checkpoint) Store parameters."

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
        member val CheckpointInterval =     TimeSpan.FromHours 1.
        member val TargetStoreArgs : TargetStoreArguments =
            match a.GetSubCommand() with
            | Cosmos cosmos -> TargetStoreArguments.Cosmos(Args.Cosmos.Arguments (c, cosmos))
            | Dynamo dynamo -> TargetStoreArguments.Dynamo(Args.Dynamo.Arguments (c, dynamo))
            | _ -> Args.missingArg "Must specify `cosmos` or `dynamo` target store when source is `esdb`"

[<RequireQualifiedAccess; NoComparison; NoEquality>]
type Arguments = Cosmos of Cosmos.Arguments | Dynamo of Dynamo.Arguments | Esdb of Esdb.Arguments
let verboseRequested = function
    | Arguments.Cosmos a -> a.Verbose
    | Arguments.Dynamo a -> a.Verbose
    | Arguments.Esdb a -> a.Verbose
let dumpMetrics = function
    | Arguments.Cosmos _ -> Equinox.CosmosStore.Core.Log.InternalMetrics.dump
    | Arguments.Dynamo _ -> Equinox.DynamoStore.Core.Log.InternalMetrics.dump
    | Arguments.Esdb _ -> Equinox.EventStoreDb.Log.InternalMetrics.dump

(*
    type [<NoEquality; NoComparison>] Parameters =
        | [<AltCommandLine "-Z"; Unique>]   FromTail
        | [<AltCommandLine "-g"; Unique>]   Gorge of int
        | [<AltCommandLine "-t"; Unique>]   Tail of intervalS: float
        | [<AltCommandLine "--force"; Unique>] ForceRestart
        | [<AltCommandLine "-m"; Unique>]   BatchSize of int

        | [<AltCommandLine "-mim"; Unique>] MinBatchSize of int
        | [<AltCommandLine "-pos"; Unique>] Position of int64
        | [<AltCommandLine "-c"; Unique>]   Chunk of int
        | [<AltCommandLine "-pct"; Unique>] Percent of float

        | [<AltCommandLine "-V">]           Verbose
        | [<AltCommandLine "-o">]           Timeout of float
        | [<AltCommandLine "-r">]           Retries of int
        | [<AltCommandLine "-oh">]          HeartbeatTimeout of float
        | [<AltCommandLine "-h">]           Host of string
        | [<AltCommandLine "-x">]           Port of int
        | [<AltCommandLine "-u">]           Username of string
        | [<AltCommandLine "-p">]           Password of string
        | [<AltCommandLine "-hp">]          ProjHost of string
        | [<AltCommandLine "-xp">]          ProjPort of int
        | [<AltCommandLine "-up">]          ProjUsername of string
        | [<AltCommandLine "-pp">]          ProjPassword of string

        interface IArgParserTemplate with
            member a.Usage = a |> function
                | FromTail ->               "Start the processing from the Tail"
                | Gorge _ ->                "Request Parallel readers phase during initial catchup, running one chunk (256MB) apart. Default: off"
                | Tail _ ->                 "attempt to read from tail at specified interval in Seconds. Default: 1"
                | ForceRestart _ ->         "Forget the current committed position; start from (and commit) specified position. Default: start from specified position or resume from committed."
                | BatchSize _ ->            "maximum item count to request from feed. Default: 4096"
                | MinBatchSize _ ->         "minimum item count to drop down to in reaction to read failures. Default: 512"
                | Position _ ->             "EventStore $all Stream Position to commence from"
                | Chunk _ ->                "EventStore $all Chunk to commence from"
                | Percent _ ->              "EventStore $all Stream Position to commence from (as a percentage of current tail position)"

                | Verbose ->                "Include low level Store logging."
                | Host _ ->                 "TCP mode: specify EventStore hostname to connect to directly. Clustered mode: use Gossip protocol against all A records returned from DNS query. (optional if environment variable EQUINOX_ES_HOST specified)"
                | Port _ ->                 "specify EventStore custom port. Uses value of environment variable EQUINOX_ES_PORT if specified. Defaults for Cluster and Direct TCP/IP mode are 30778 and 1113 respectively."
                | Username _ ->             "specify username for EventStore. (optional if environment variable EQUINOX_ES_USERNAME specified)"
                | Password _ ->             "specify Password for EventStore. (optional if environment variable EQUINOX_ES_PASSWORD specified)"
                | ProjHost _ ->             "TCP mode: specify Projection EventStore hostname to connect to directly. Clustered mode: use Gossip protocol against all A records returned from DNS query. Defaults to value of es host (-h) unless environment variable EQUINOX_ES_PROJ_HOST is specified."
                | ProjPort _ ->             "specify Projection EventStore custom port. Defaults to value of es port (-x) unless environment variable EQUINOX_ES_PROJ_PORT is specified."
                | ProjUsername _ ->         "specify username for Projection EventStore. Defaults to value of es user (-u) unless environment variable EQUINOX_ES_PROJ_USERNAME is specified."
                | ProjPassword _ ->         "specify Password for Projection EventStore. Defaults to value of es password (-p) unless environment variable EQUINOX_ES_PROJ_PASSWORD is specified."
                | Timeout _ ->              "specify operation timeout in seconds. Default: 20."
                | Retries _ ->              "specify operation retries. Default: 3."
                | HeartbeatTimeout _ ->     "specify heartbeat timeout in seconds. Default: 1.5."

    and Arguments(c : Configuration, a : ParseResults<Parameters>) =

        let ts (x : TimeSpan) = x.TotalSeconds
        let discovery (host, port) =
            match port with
            | None ->   Discovery.GossipDns            host
            | Some p -> Discovery.GossipDnsCustomPort (host, p)
        let host =                          a.TryGetResult Host |> Option.defaultWith (fun () -> c.EventStoreHost)
        let port =                          a.TryGetResult Port |> Option.orElseWith (fun () -> c.EventStorePort)
        let user =                          a.TryGetResult Username |> Option.defaultWith (fun () -> c.EventStoreUsername)
        let password =                      a.TryGetResult Password |> Option.defaultWith (fun () -> c.EventStorePassword)
        member val Gorge =                  a.TryGetResult Gorge
        member val TailInterval =           a.GetResult(Tail, 1.) |> TimeSpan.FromSeconds
        member val ForceRestart =           a.Contains ForceRestart
        member val StartingBatchSize =      a.GetResult(BatchSize, 4096)
        member val MinBatchSize =           a.GetResult(MinBatchSize, 512)
        member val StartPos =
            match a.TryGetResult Position, a.TryGetResult Chunk, a.TryGetResult Percent, a.Contains EsSourceParameters.FromTail with
            | Some p, _, _, _ ->            Absolute p
            | _, Some c, _, _ ->            StartPos.Chunk c
            | _, _, Some p, _ ->            Percentage p
            | None, None, None, true ->     StartPos.TailOrCheckpoint
            | None, None, None, _ ->        StartPos.StartOrCheckpoint
        member val Host =                   host
        member val Port =                   port
        member val User =                   user
        member val Password =               password
        member val ProjPort =               match a.TryGetResult ProjPort with
                                            | Some x -> Some x
                                            | None -> c.EventStoreProjectionPort |> Option.orElse port
        member val ProjHost =               match a.TryGetResult ProjHost with
                                            | Some x -> x
                                            | None -> c.EventStoreProjectionHost |> Option.defaultValue host
        member val ProjUser =               match a.TryGetResult ProjUsername with
                                            | Some x -> x
                                            | None -> c.EventStoreProjectionUsername |> Option.defaultValue user
        member val ProjPassword =           match a.TryGetResult ProjPassword with
                                            | Some x -> x
                                            | None -> c.EventStoreProjectionPassword |> Option.defaultValue password
        member val Retries =                a.GetResult(EsSourceParameters.Retries, 3)
        member val Timeout =                a.GetResult(EsSourceParameters.Timeout, 20.) |> TimeSpan.FromSeconds
        member val Heartbeat =              a.GetResult(HeartbeatTimeout, 1.5) |> TimeSpan.FromSeconds

        member x.ConnectProj(log: ILogger, storeLog: ILogger, appName, nodePreference) =
            let discovery = discovery (x.ProjHost, x.ProjPort)
            log.ForContext("projHost", x.ProjHost).ForContext("projPort", x.ProjPort)
                .Information("Projection EventStore {discovery} heartbeat: {heartbeat}s Timeout: {timeout}s Retries {retries}",
                    discovery, ts x.Heartbeat, ts x.Timeout, x.Retries)
            let log=if storeLog.IsEnabled Serilog.Events.LogEventLevel.Debug then Logger.SerilogVerbose storeLog else Logger.SerilogNormal storeLog
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Connector(x.ProjUser, x.ProjPassword, x.Timeout, x.Retries, log=log, heartbeatTimeout=x.Heartbeat, tags=tags)
                .Connect(appName + "-Proj", discovery, nodePreference) |> Async.RunSynchronously

        member x.Connect(log: ILogger, storeLog: ILogger, appName, connectionStrategy) =
            let discovery = discovery (x.Host, x.Port)
            log.ForContext("host", x.Host).ForContext("port", x.Port)
                .Information("EventStore {discovery} heartbeat: {heartbeat}s Timeout: {timeout}s Retries {retries}",
                    discovery, ts x.Heartbeat, ts x.Timeout, x.Retries)
            let log=if storeLog.IsEnabled Serilog.Events.LogEventLevel.Debug then Logger.SerilogVerbose storeLog else Logger.SerilogNormal storeLog
            let tags=["M", Environment.MachineName; "I", Guid.NewGuid() |> string]
            Connector(x.User, x.Password, x.Timeout, x.Retries, log=log, heartbeatTimeout=x.Heartbeat, tags=tags)
                .Establish(appName, discovery, connectionStrategy) |> Async.RunSynchronously
    *)
