using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Requests;
using MediatR;
using Orders.Shipments.Commands;
using RestSharp;

namespace Orders.Shipments
{
    public class ShipmentsCommandHandler:
        ICommandHandler<SendPackage>
    {
        private readonly ExternalServicesConfig externalServicesConfig;
        private readonly IExternalCommandBus externalCommandBus;

        public ShipmentsCommandHandler(ExternalServicesConfig externalServicesConfig, IExternalCommandBus externalCommandBus)
        {
            this.externalServicesConfig = externalServicesConfig;
            this.externalCommandBus = externalCommandBus;
        }

        public async Task<Unit> Handle(SendPackage command, CancellationToken cancellationToken)
        {
            await externalCommandBus.Post(
                externalServicesConfig.PaymentsUrl,
                "payments",
                command,
                cancellationToken);

            return Unit.Value;
        }
    }
}
