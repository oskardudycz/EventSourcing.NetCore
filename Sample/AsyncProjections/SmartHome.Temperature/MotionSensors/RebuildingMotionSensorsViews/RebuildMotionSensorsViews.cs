using Core.Commands;
using Marten;

namespace SmartHome.Temperature.MotionSensors.RebuildingMotionSensorsViews;

public record RebuildMotionSensorsViews;

public class HandleRebuildMotionSensorsViews :
    ICommandHandler<RebuildMotionSensorsViews>
{
    private readonly IDocumentSession session;

    public HandleRebuildMotionSensorsViews(
        IDocumentSession session
    )
    {
        this.session = session;
    }

    public async Task Handle(RebuildMotionSensorsViews command, CancellationToken cancellationToken)
    {
        using var daemon = await session.DocumentStore.BuildProjectionDaemonAsync();
        await daemon.RebuildProjection<MotionSensor>(cancellationToken);
    }
}
