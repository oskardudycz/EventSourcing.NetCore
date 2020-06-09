using System;
using Ardalis.GuardClauses;
using Core.Commands;

namespace SmartHome.Temperature.MotionSensors.Commands
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
            Guard.Against.Default(motionSensorId, nameof(motionSensorId));

            return new InstallMotionSensor(motionSensorId);
        }
    }
}
