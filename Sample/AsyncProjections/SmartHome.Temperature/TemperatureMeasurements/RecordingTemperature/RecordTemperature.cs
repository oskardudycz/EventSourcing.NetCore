using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature
{
    public class RecordTemperature : ICommand
    {
        public Guid MeasurementId { get; }

        public decimal Temperature { get; }

        private RecordTemperature(Guid measurementId, decimal temperature)
        {
            MeasurementId = measurementId;
            Temperature = temperature;
        }

        public static RecordTemperature Create(Guid measurementId, decimal temperature)
        {
            Guard.Against.Default(measurementId, nameof(measurementId));
            Guard.Against.OutOfRange(temperature, nameof(temperature), -273, Decimal.MaxValue);

            return new RecordTemperature(measurementId, temperature);
        }
    }

    public class HandleRecordTemperature:
        ICommandHandler<RecordTemperature>
    {
        private readonly IRepository<TemperatureMeasurement> repository;

        public HandleRecordTemperature(
            IRepository<TemperatureMeasurement> repository
        )
        {
            Guard.Against.Null(repository, nameof(repository));

            this.repository = repository;
        }

        public async Task<Unit> Handle(RecordTemperature command, CancellationToken cancellationToken)
        {
            Guard.Against.Null(command, nameof(command));

            var reservation = await repository.Find(command.MeasurementId, cancellationToken);

            reservation!.Record(command.Temperature);

            await repository.Update(reservation, cancellationToken);

            return Unit.Value;
        }
    }
}
