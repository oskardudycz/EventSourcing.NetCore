using Core.Commands;

namespace SmartHome.Temperature.MotionSensors.Commands
{
    public class RebuildMotionSensorsViews : ICommand
    {
        private RebuildMotionSensorsViews(){}

        public static RebuildMotionSensorsViews Create()
        {
            return new RebuildMotionSensorsViews();
        }
    }
}
