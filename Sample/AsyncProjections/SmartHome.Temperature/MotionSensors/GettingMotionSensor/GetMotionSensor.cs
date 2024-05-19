using Core.Queries;
using Marten;

namespace SmartHome.Temperature.MotionSensors.GettingMotionSensor;

public class GetMotionSensors
{
    private GetMotionSensors(){ }

    public static GetMotionSensors Create() => new();
}

public class HandleGetMotionSensors(IDocumentSession querySession)
    : IQueryHandler<GetMotionSensors, IReadOnlyList<MotionSensor>>
{
    public Task<IReadOnlyList<MotionSensor>> Handle(GetMotionSensors request, CancellationToken cancellationToken) =>
        querySession.Query<MotionSensor>().ToListAsync(cancellationToken);
}
