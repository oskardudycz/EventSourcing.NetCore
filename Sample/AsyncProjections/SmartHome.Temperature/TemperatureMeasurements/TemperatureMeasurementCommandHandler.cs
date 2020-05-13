using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Storage;
using MediatR;
using SmartHome.Temperature.TemperatureMeasurements.Commands;

namespace SmartHome.Temperature.TemperatureMeasurements
{
    public class TemperatureSummaryCommandHandler:
        ICommandHandler<StartTemperatureMeasurement>,
        ICommandHandler<RecordTemperature>
    {
        private readonly IRepository<TemperatureMeasurement> repository;

        public TemperatureSummaryCommandHandler(
            IRepository<TemperatureMeasurement> repository
        )
        {
            Guard.Against.Null(repository, nameof(repository));

            this.repository = repository;
        }

        public async Task<Unit> Handle(StartTemperatureMeasurement command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command, nameof(command));

            var reservation = TemperatureMeasurement.Start(
                command.MeasurementId
            );

            await repository.Add(reservation, cancellationToken);

            return Unit.Value;
        }

        public async Task<Unit> Handle(RecordTemperature command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command, nameof(command));

            var reservation = await repository.Find(command.MeasurementId, cancellationToken);

            reservation.Record(command.Temperature);

            await repository.Update(reservation, cancellationToken);

            return Unit.Value;
        }
    }
}
