module ECommerce.Reactor.ConfirmedHandler

open ECommerce.Domain

type Service(carts : ShoppingCart.Service, ingester : ConfirmedIngester.Service) =

    member _.TrySummarizeConfirmed(cartId) : Async<bool> = async {
        let! cartSummary, originEpoch = carts.SummarizeWithOriginEpoch(cartId, ingester.ActiveIngestionEpochId)
        return! ingester.TryIngestCartSummary(originEpoch, ConfirmedEpoch.ofShoppingCartView cartId cartSummary) }

module Config =

    let create_ linger (sourceStore, cosmosStore) =
        let carts = ShoppingCart.Config.create sourceStore
        let ingester = ConfirmedIngester.Config.create linger cosmosStore
        Service(carts, ingester)
    let create store =
        let defaultLinger = System.TimeSpan.FromMilliseconds 200.
        create_ defaultLinger store
