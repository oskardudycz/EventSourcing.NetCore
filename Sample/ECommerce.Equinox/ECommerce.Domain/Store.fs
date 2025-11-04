module Store

module Metrics =

    let log = Serilog.Log.ForContext("isMetric", true)

let createDecider category = Equinox.Decider.forStream Metrics.log category

module Memory =

    let create name codec initial fold store: Equinox.Category<_, _, _> =
        Equinox.MemoryStore.MemoryStoreCategory(store, name, FsCodec.Encoder.Uncompressed codec, fold, initial)

module Codec =

    open FsCodec.SystemTextJson
    let private defaultOptions = Options.Create(autoTypeSafeEnumToJsonString = true)
    let gen<'t when 't :> TypeShape.UnionContract.IUnionContract> =
        Codec.Create<'t>(options = defaultOptions)
    let genJsonElement<'t when 't :> TypeShape.UnionContract.IUnionContract> =
        CodecJsonElement.Create<'t>(options = defaultOptions)

let private defaultCacheDuration = System.TimeSpan.FromMinutes 20.

module Cosmos =

    let private createCached name codec initial fold accessStrategy (context, cache) =
        let cacheStrategy = Equinox.CachingStrategy.SlidingWindow (cache, defaultCacheDuration)
        Equinox.CosmosStore.CosmosStoreCategory(context, name,  FsCodec.SystemTextJson.Encoder.Compressed codec, fold, initial, accessStrategy, cacheStrategy)

    let createUnoptimized name codec initial fold (context, cache) =
        let accessStrategy = Equinox.CosmosStore.AccessStrategy.Unoptimized
        createCached name codec initial fold accessStrategy (context, cache)

    let createSnapshotted name codec initial fold (isOrigin, toSnapshot) (context, cache) =
        let accessStrategy = Equinox.CosmosStore.AccessStrategy.Snapshot (isOrigin, toSnapshot)
        createCached name codec initial fold accessStrategy (context, cache)

    let createRollingState name codec initial fold toSnapshot (context, cache) =
        let accessStrategy = Equinox.CosmosStore.AccessStrategy.RollingState toSnapshot
        createCached name codec initial fold accessStrategy (context, cache)

module Dynamo =

    let private createCached name codec initial fold accessStrategy (context, cache) =
        let cacheStrategy = Equinox.CachingStrategy.SlidingWindow (cache, defaultCacheDuration)
        Equinox.DynamoStore.DynamoStoreCategory(context, name, FsCodec.Encoder.Compressed codec, fold, initial, accessStrategy, cacheStrategy)

    let createUnoptimized name codec initial fold (context, cache) =
        let accessStrategy = Equinox.DynamoStore.AccessStrategy.Unoptimized
        createCached name codec initial fold accessStrategy (context, cache)

    let createSnapshotted name codec initial fold (isOrigin, toSnapshot) (context, cache) =
        let accessStrategy = Equinox.DynamoStore.AccessStrategy.Snapshot (isOrigin, toSnapshot)
        createCached name codec initial fold accessStrategy (context, cache)

    let createRollingState name codec initial fold toSnapshot (context, cache) =
        let accessStrategy = Equinox.DynamoStore.AccessStrategy.RollingState toSnapshot
        createCached name codec initial fold accessStrategy (context, cache)

module Esdb =

    let private createCached name codec initial fold accessStrategy (context, cache) =
        let cacheStrategy = Equinox.CachingStrategy.SlidingWindow (cache, defaultCacheDuration)
        Equinox.EventStoreDb.EventStoreCategory(context, name, codec, fold, initial, accessStrategy, cacheStrategy)
    let createUnoptimized name codec initial fold (context, cache) =
        createCached name codec initial fold Equinox.EventStoreDb.AccessStrategy.Unoptimized (context, cache)
    let createLatestKnownEvent name codec initial fold (context, cache) =
        createCached name codec initial fold Equinox.EventStoreDb.AccessStrategy.LatestKnownEvent (context, cache)

module Sss =

    let private createCached name codec initial fold accessStrategy (context, cache) =
        let cacheStrategy = Equinox.CachingStrategy.SlidingWindow (cache, defaultCacheDuration)
        Equinox.SqlStreamStore.SqlStreamStoreCategory(context, name, codec, fold, initial, accessStrategy, cacheStrategy)
    let createUnoptimized name codec initial fold (context, cache) =
        createCached name codec initial fold Equinox.SqlStreamStore.AccessStrategy.Unoptimized (context, cache)
    let createLatestKnownEvent name codec initial fold (context, cache) =
        createCached name codec initial fold Equinox.SqlStreamStore.AccessStrategy.LatestKnownEvent (context, cache)

[<NoComparison; NoEquality; RequireQualifiedAccess>]
type Config =
    | Memory of Equinox.MemoryStore.VolatileStore<struct (int * System.ReadOnlyMemory<byte>)>
    | Cosmos of Equinox.CosmosStore.CosmosStoreContext * Equinox.Cache
    | Dynamo of Equinox.DynamoStore.DynamoStoreContext * Equinox.Cache
    | Esdb of Equinox.EventStoreDb.EventStoreContext * Equinox.Cache
    | Sss of Equinox.SqlStreamStore.SqlStreamStoreContext * Equinox.Cache
