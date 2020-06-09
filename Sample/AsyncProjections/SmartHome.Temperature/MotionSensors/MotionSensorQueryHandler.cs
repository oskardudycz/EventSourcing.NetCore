using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Queries;
using Marten;
using SmartHome.Temperature.MotionSensors.Queries;

namespace SmartHome.Temperature.MotionSensors
{
    public class MotionSensorQueryHandler : IQueryHandler<GetMotionSensors, IReadOnlyList<MotionSensor>>
    {
        private readonly IDocumentSession querySession;

        public MotionSensorQueryHandler(IDocumentSession querySession)
        {
            Guard.Against.Null(querySession, nameof(querySession));

            this.querySession = querySession;
        }

        public Task<IReadOnlyList<MotionSensor>> Handle(GetMotionSensors request, CancellationToken cancellationToken)
        {
            return querySession.Query<MotionSensor>().ToListAsync(cancellationToken);
        }
    }
}
