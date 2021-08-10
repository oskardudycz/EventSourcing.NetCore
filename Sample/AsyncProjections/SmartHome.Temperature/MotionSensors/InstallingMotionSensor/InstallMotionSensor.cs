using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Repositories;
using MediatR;

namespace SmartHome.Temperature.MotionSensors.InstallingMotionSensor
{
    public class InstallMotionSensor : ICommand
    {
        public Guid MotionSensorId { get; }

        private InstallMotionSensor(
            Guid motionSensorId
        )
        {
            MotionSensorId = motionSensorId;
        }

        public static InstallMotionSensor Create(
            Guid motionSensorId
        )
        {
            if (motionSensorId == Guid.Empty)
                throw new ArgumentOutOfRangeException(nameof(motionSensorId));

            return new InstallMotionSensor(motionSensorId);
        }
    }

    public class HandleInstallMotionSensor :
        ICommandHandler<InstallMotionSensor>
    {
        private readonly IRepository<MotionSensor> repository;

        public HandleInstallMotionSensor(
            IRepository<MotionSensor> repository
        )
        {
            this.repository = repository;
        }

        public async Task<Unit> Handle(InstallMotionSensor command, CancellationToken cancellationToken)
        {
            var reservation = MotionSensor.Install(
                command.MotionSensorId
            );

            await repository.Add(reservation, cancellationToken);

            return Unit.Value;
        }
    }
}
