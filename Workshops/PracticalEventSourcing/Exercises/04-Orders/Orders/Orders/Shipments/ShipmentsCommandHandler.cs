using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using MediatR;
using Orders.Shipments.Commands;
using RestSharp;

namespace Orders.Shipments
{
    public class ShipmentsCommandHandler:
        ICommandHandler<SendPackage>
    {
        private readonly ExternalServicesConfig externalServicesConfig;

        public ShipmentsCommandHandler(ExternalServicesConfig externalServicesConfig)
        {
            this.externalServicesConfig = externalServicesConfig;
        }

        public async Task<Unit> Handle(SendPackage command, CancellationToken cancellationToken)
        {
            var client = new RestClient(externalServicesConfig.PaymentsUrl);

            var request = new RestRequest("payments", DataFormat.Json);
            request.AddJsonBody(command);

            await client.PostAsync<dynamic>(request, cancellationToken);

            return Unit.Value;
        }
    }
}
