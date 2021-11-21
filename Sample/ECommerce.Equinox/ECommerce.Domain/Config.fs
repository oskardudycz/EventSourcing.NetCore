module ECommerce.Domain.Config

let log = Serilog.Log.ForContext("isMetric", true)
let createDecider stream = Equinox.Decider(log, stream, maxAttempts = 3)

module Memory =

    let create codec initial fold store =
        Equinox.MemoryStore.MemoryStoreCategory(store, codec, fold, initial)

module Cosmos =

    let private createCached codec initial fold accessStrategy (context, cache) =
        let cacheStrategy = Equinox.CosmosStore.CachingStrategy.SlidingWindow (cache, System.TimeSpan.FromMinutes 20.)
        Equinox.CosmosStore.CosmosStoreCategory(context, codec, fold, initial, cacheStrategy, accessStrategy)

    let createUnoptimized codec initial fold (context, cache) =
        let accessStrategy = Equinox.CosmosStore.AccessStrategy.Unoptimized
        createCached codec initial fold accessStrategy (context, cache)

//    let createSnapshotted codec initial fold (isOrigin, toSnapshot) (context, cache) =
//        let accessStrategy = Equinox.CosmosStore.AccessStrategy.Snapshot (isOrigin, toSnapshot)
//        createCached codec initial fold accessStrategy (context, cache)
//
    let createRollingState codec initial fold toSnapshot (context, cache) =
        let accessStrategy = Equinox.CosmosStore.AccessStrategy.RollingState toSnapshot
        createCached codec initial fold accessStrategy (context, cache)

[<NoComparison; NoEquality; RequireQualifiedAccess>]
type Store<'t> =
    | Memory of Equinox.MemoryStore.VolatileStore<'t>
    | Cosmos of Equinox.CosmosStore.CosmosStoreContext * Equinox.Core.ICache
