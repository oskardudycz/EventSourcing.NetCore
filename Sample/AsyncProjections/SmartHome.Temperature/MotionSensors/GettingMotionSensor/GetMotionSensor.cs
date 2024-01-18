using Core.Queries;
using Marten;

namespace SmartHome.Temperature.MotionSensors.GettingMotionSensor;

public class GetMotionSensors
{
    private GetMotionSensors(){ }

    public static GetMotionSensors Create() => new();
}

public class HandleGetMotionSensors : IQueryHandler<GetMotionSensors, IReadOnlyList<MotionSensor>>
{
    private readonly IDocumentSession querySession;

    public HandleGetMotionSensors(IDocumentSession querySession)
    {
        this.querySession = querySession;
    }

    public Task<IReadOnlyList<MotionSensor>> Handle(GetMotionSensors request, CancellationToken cancellationToken)
    {
        return querySession.Query<MotionSensor>().ToListAsync(cancellationToken);
    }
}
