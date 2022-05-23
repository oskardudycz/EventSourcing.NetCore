module ECommerce.Infrastructure.CheckpointStore

[<RequireQualifiedAccess; NoComparison; NoEquality>]
type Config =
    | Cosmos of Equinox.CosmosStore.CosmosStoreContext * Equinox.Core.ICache
    | Dynamo of Equinox.DynamoStore.DynamoStoreContext * Equinox.Core.ICache

let create (consumerGroup, checkpointInterval) storeLog : Config -> Propulsion.Feed.IFeedCheckpointStore = function
    | Config.Cosmos (context, cache) ->
        Propulsion.Feed.ReaderCheckpoint.CosmosStore.create storeLog (consumerGroup, checkpointInterval) (context, cache)
    | Config.Dynamo (context, cache) ->
        Propulsion.Feed.ReaderCheckpoint.DynamoStore.create storeLog (consumerGroup, checkpointInterval) (context, cache)
