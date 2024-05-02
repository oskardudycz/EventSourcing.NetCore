using Core.Commands;
using Marten;

namespace SmartHome.Temperature.MotionSensors.RebuildingMotionSensorsViews;

public record RebuildMotionSensorsViews;

public class HandleRebuildMotionSensorsViews(IDocumentSession session):
    ICommandHandler<RebuildMotionSensorsViews>
{
    public async Task Handle(RebuildMotionSensorsViews command, CancellationToken ct)
    {
        using var daemon = await session.DocumentStore.BuildProjectionDaemonAsync();
        await daemon.RebuildProjectionAsync<MotionSensor>(ct);
    }
}
