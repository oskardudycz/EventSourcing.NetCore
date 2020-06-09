using System;
using Ardalis.GuardClauses;
using Core.Events;
using Newtonsoft.Json;

namespace SmartHome.Temperature.MotionSensors.Events
{
    public class MotionSensorInstalled : IEvent
    {
        public Guid MotionSensorId { get; }

        public DateTime InstalledAt { get; }

        [JsonConstructor]
        private MotionSensorInstalled(
            Guid motionSensorId,
            DateTime installedAt
        )
        {
            MotionSensorId = motionSensorId;
            InstalledAt = installedAt;
        }

        public static MotionSensorInstalled Create(
            Guid motionSensorId,
            DateTime installedAt
        )
        {
            Guard.Against.Default(motionSensorId, nameof(motionSensorId));
            Guard.Against.Default(installedAt, nameof(installedAt));

            return new MotionSensorInstalled(motionSensorId, installedAt);
        }
    }
}
