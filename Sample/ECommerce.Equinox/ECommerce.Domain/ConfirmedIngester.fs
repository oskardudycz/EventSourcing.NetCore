module ECommerce.Domain.ConfirmedIngester

type Service internal (tip : ExactlyOnceIngester.Service<_, _, _, _>) =

    /// Slot the item into the series of epochs.
    /// Returns items that actually got added (i.e. may be empty if it was an idempotent retry).
    member _.IngestItems(originEpochId, items) : Async<seq<_>>=
        tip.IngestMany(originEpochId, items)

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
    let create store =
        let defaultLinger = System.TimeSpan.FromMilliseconds 200.
        let maxItemsPerEpoch = 10_000
        create_ maxItemsPerEpoch defaultLinger store
