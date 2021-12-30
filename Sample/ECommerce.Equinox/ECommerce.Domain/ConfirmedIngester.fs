module ECommerce.Domain.ConfirmedIngester

type Service internal (tip : ExactlyOnceIngester.Service<_, _, _, _>) =

    member _.IngestMany(originEpochId, cartSummaries) : Async<CartId seq> =
        tip.IngestMany(originEpochId, cartSummaries)

    /// Slot the item into the series of epochs.
    /// Returns true if it got added this time, i.e. idempotent retries don't count
    member x.TryIngestCartSummary(originEpochId, cartSummary : ConfirmedEpoch.Events.Cart) : Async<bool> = async {
        let! ingested = x.IngestMany(originEpochId, [| cartSummary |])
        return ingested |> Seq.contains cartSummary.cartId }

    /// Efficiently determine a valid ingestion origin epoch
    member _.ActiveIngestionEpochId() =
        tip.ActiveIngestionEpochId()

module Config =

    let create_ maxItemsPerEpoch linger store =
        let series = ConfirmedSeries.Config.create store
        let epochs = ConfirmedEpoch.Config.create maxItemsPerEpoch store
        let log = Serilog.Log.ForContext<Service>()
        let tip = ExactlyOnceIngester.create log linger (series.ReadIngestionEpochId, series.MarkIngestionEpochId) (epochs.Ingest, Array.toSeq)
        Service(tip)
    let create linger store =
        let maxItemsPerEpoch = 10_000
        create_ maxItemsPerEpoch linger store
