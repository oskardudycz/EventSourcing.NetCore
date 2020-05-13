using System.Collections.Generic;
using Core.Queries;

namespace SmartHome.Temperature.TemperatureMeasurements.Queries
{
    public class GetTemperatureMeasurements: IQuery<IReadOnlyList<TemperatureMeasurement>>
    {
        public static GetTemperatureMeasurements Create()
        {
            return new GetTemperatureMeasurements();
        }
    }
}
