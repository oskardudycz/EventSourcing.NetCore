using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using MediatR;
using Orders.Payments.Commands;
using RestSharp;

namespace Orders.Payments
{
    public class PaymentsCommandHandler:
        ICommandHandler<DiscardPayment>,
        ICommandHandler<RequestPayment>
    {
        private readonly ExternalServicesConfig externalServicesConfig;

        public PaymentsCommandHandler(ExternalServicesConfig externalServicesConfig)
        {
            this.externalServicesConfig = externalServicesConfig;
        }

        public async Task<Unit> Handle(RequestPayment command, CancellationToken cancellationToken)
        {
            var client = new RestClient(externalServicesConfig.PaymentsUrl);

            var request = new RestRequest("payments", DataFormat.Json);
            request.AddJsonBody(command);

            await client.PostAsync<dynamic>(request, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(DiscardPayment command, CancellationToken cancellationToken)
        {
            var client = new RestClient(externalServicesConfig.PaymentsUrl);

            var request = new RestRequest("payments", DataFormat.Json);
            request.AddJsonBody(command);

            await client.DeleteAsync<dynamic>(request, cancellationToken);

            return Unit.Value;
        }
    }
}
