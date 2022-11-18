using Core.Commands;
using Core.Requests;
using MediatR;
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

public class HandleSendPackage:
    ICommandHandler<SendPackage>
{
    private readonly ExternalServicesConfig externalServicesConfig;
    private readonly IExternalCommandBus externalCommandBus;

    public HandleSendPackage(ExternalServicesConfig externalServicesConfig, IExternalCommandBus externalCommandBus)
    {
        this.externalServicesConfig = externalServicesConfig;
        this.externalCommandBus = externalCommandBus;
    }

    public async Task Handle(SendPackage command, CancellationToken cancellationToken)
    {
        await externalCommandBus.Post(
            externalServicesConfig.ShipmentsUrl!,
            "shipments",
            command,
            cancellationToken);
    }
}
