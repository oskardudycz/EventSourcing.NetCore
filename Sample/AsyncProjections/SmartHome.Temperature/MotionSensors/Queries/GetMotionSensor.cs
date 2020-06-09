using System.Collections.Generic;
using Core.Queries;

namespace SmartHome.Temperature.MotionSensors.Queries
{
    public class GetMotionSensors : IQuery<IReadOnlyList<MotionSensor>>
    {
        private GetMotionSensors(){ }

        public static GetMotionSensors Create()
        {
            return new GetMotionSensors();
        }

    }
}
