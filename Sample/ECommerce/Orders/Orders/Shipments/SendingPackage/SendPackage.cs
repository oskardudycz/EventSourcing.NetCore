using Core.Commands;
using Core.Requests;
using Orders.Products;

namespace Orders.Shipments.SendingPackage;

public record SendPackage(
    Guid OrderId,
    IReadOnlyList<ProductItem> ProductItems
)
{
    public static SendPackage Create(
        Guid orderId,
        IReadOnlyList<ProductItem> productItems
    )
    {
        if (orderId == Guid.Empty)
            throw new ArgumentOutOfRangeException(nameof(orderId));
        if (productItems.Count == 0)
            throw new ArgumentOutOfRangeException(nameof(productItems.Count));

        return new SendPackage(orderId, productItems);
    }
}

public class HandleSendPackage(ExternalServicesConfig externalServicesConfig, IExternalCommandBus externalCommandBus)
    :
        ICommandHandler<SendPackage>
{
    public async Task Handle(SendPackage command, CancellationToken ct)
    {
        await externalCommandBus.Post(
            externalServicesConfig.ShipmentsUrl!,
            "shipments",
            command,
            ct);
    }
}
