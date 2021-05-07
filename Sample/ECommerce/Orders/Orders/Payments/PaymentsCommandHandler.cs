using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Requests;
using MediatR;
using Orders.Payments.Commands;

namespace Orders.Payments
{
    public class PaymentsCommandHandler:
        ICommandHandler<DiscardPayment>,
        ICommandHandler<RequestPayment>
    {
        private readonly ExternalServicesConfig externalServicesConfig;
        private readonly IExternalCommandBus externalCommandBus;

        public PaymentsCommandHandler(ExternalServicesConfig externalServicesConfig,
            IExternalCommandBus externalCommandBus)
        {
            this.externalServicesConfig = externalServicesConfig;
            this.externalCommandBus = externalCommandBus;
        }

        public async Task<Unit> Handle(RequestPayment command, CancellationToken cancellationToken)
        {
            await externalCommandBus.Post(
                externalServicesConfig.PaymentsUrl!,
                "payments",
                command,
                cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(DiscardPayment command, CancellationToken cancellationToken)
        {
            await externalCommandBus.Delete(
                externalServicesConfig.PaymentsUrl!,
                "payments",
                command,
                cancellationToken);

            return Unit.Value;
        }
    }
}
