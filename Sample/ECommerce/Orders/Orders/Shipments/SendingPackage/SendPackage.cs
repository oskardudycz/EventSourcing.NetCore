using Core.Commands;
using Core.Requests;
using MediatR;
using Orders.Products;

namespace Orders.Shipments.SendingPackage;

public class SendPackage : ICommand
{
    public Guid OrderId { get; }

    public IReadOnlyList<ProductItem> ProductItems { get; }

    private SendPackage(
        Guid orderId,
        IReadOnlyList<ProductItem> productItems
    )
    {
        OrderId = orderId;
        ProductItems = productItems;
    }

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
    private readonly IHttpExternalCommandBus httpExternalCommandBus;

    public HandleSendPackage(ExternalServicesConfig externalServicesConfig, IHttpExternalCommandBus httpExternalCommandBus)
    {
        this.externalServicesConfig = externalServicesConfig;
        this.httpExternalCommandBus = httpExternalCommandBus;
    }

    public async Task<Unit> Handle(SendPackage command, CancellationToken cancellationToken)
    {
        await httpExternalCommandBus.Post(
            externalServicesConfig.ShipmentsUrl!,
            "shipments",
            command,
            cancellationToken);

        return Unit.Value;
    }
}
