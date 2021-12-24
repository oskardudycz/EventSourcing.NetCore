module ECommerce.Reactor.ShoppingCartSummaryHandler

open ECommerce.Domain

type Service(source : ShoppingCart.Service, destination : ShoppingCartSummary.Service) =

    member _.TryIngestSummary(cartId) : Async<bool * int64> = async {
        match! source.SummarizeWithVersion(cartId) with
        | Some view, version' ->
            let! worked = destination.TryIngest(cartId, version', view)
            return worked, version'
        | None, version' -> return false, version' }

module Config =

    let create (sourceStore, destinationStore) =
        let source = ShoppingCart.Config.create sourceStore
        let destination = ShoppingCartSummary.Config.create destinationStore
        Service(source, destination)
