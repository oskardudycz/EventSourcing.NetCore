using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Marten;
using MediatR;

namespace SmartHome.Temperature.MotionSensors.RebuildingMotionSensorsViews;

public record RebuildMotionSensorsViews: ICommand;

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

    public async Task<Unit> Handle(RebuildMotionSensorsViews command, CancellationToken cancellationToken)
    {
        using (var daemon = session.DocumentStore.BuildProjectionDaemon())
        {
            await daemon.RebuildProjection<MotionSensor>(cancellationToken);
        }
        return Unit.Value;
    }
}
