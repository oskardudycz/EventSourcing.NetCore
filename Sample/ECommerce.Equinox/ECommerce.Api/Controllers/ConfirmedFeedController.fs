namespace ECommerce.Api.Controllers

open Microsoft.AspNetCore.Mvc

open ECommerce.Domain

type TranchesDto = { activeEpochs : TrancheReferenceDto[] }
 and TrancheReferenceDto = { seriesId : ConfirmedSeriesId; epochId : ConfirmedEpochId }

module TranchesDto =

    let ofEpochId epochId =
        { activeEpochs = [| { seriesId = ConfirmedSeriesId.wellKnownId; epochId = epochId } |]}

type SliceDto = { closed : bool; carts : CartDto[]; position : ConfirmedCheckpoint; checkpoint : ConfirmedCheckpoint }
 and CartDto = { id : CartId; items : ItemDto[] }
 and ItemDto = { productId : ProductId; unitPrice : decimal; quantity : int }

module CartDto =

    let ofDto (x : ConfirmedEpoch.Events.Cart) : CartDto =
        {  id = x.cartId
           items = [| for x in x.items -> { productId = x.productId; unitPrice = x.unitPrice; quantity = x.quantity } |] }

module Checkpoint =

    let ofEpochAndOffset (epoch : ConfirmedEpochId) (offset : int) =
        ConfirmedCheckpoint.ofEpochAndOffset epoch offset

    let ofState (epochId : ConfirmedEpochId) (s : ConfirmedEpoch.Reader.StateDto) =
        ConfirmedCheckpoint.ofEpochContent epochId s.closed s.carts.Length

[<Route("api/[controller]")>]
type ConfirmedFeedController(series : ConfirmedSeries.Service, epochs : ConfirmedEpoch.Reader.Service) =
    inherit ControllerBase()

    [<HttpGet>]
    member _.ListTranches() : Async<TranchesDto> = async {
        let! active = series.ReadIngestionEpochId()
        return TranchesDto.ofEpochId active
    }

    [<HttpGet; Route("{epoch}")>]
    member _.ReadTranche(epoch : ConfirmedEpochId) : Async<SliceDto> = async {
        let! state = epochs.Read(epoch)
        // TOCONSIDER closed should control cache header
        let pos, checkpoint = Checkpoint.ofEpochAndOffset epoch 0, Checkpoint.ofState epoch state
        return { closed = state.closed; carts = Array.map CartDto.ofDto state.carts; position = pos; checkpoint = checkpoint }
    }

    [<HttpGet; Route("slice/{token?}")>]
    member _.Poll(token : System.Nullable<ConfirmedCheckpoint>) : Async<SliceDto> = async {
        let pos = if token.HasValue then token.Value else ConfirmedCheckpoint.initial
        let epochId, offset = ConfirmedCheckpoint.toEpochAndOffset pos
        let! state = epochs.Read(epochId)
        // TOCONSIDER closed should control cache header
        let pos, checkpoint = Checkpoint.ofEpochAndOffset epochId offset, Checkpoint.ofState epochId state
        return { closed = state.closed; carts = Array.skip offset state.carts |> Array.map CartDto.ofDto; position = pos; checkpoint = checkpoint }
    }
