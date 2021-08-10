using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Marten;
using MediatR;
using Npgsql;

namespace SmartHome.Temperature.MotionSensors.RebuildingMotionSensorsViews
{
    public class RebuildMotionSensorsViews : ICommand
    {
        private RebuildMotionSensorsViews(){}

        public static RebuildMotionSensorsViews Create()
        {
            return new();
        }
    }

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
            var cmd = new NpgsqlCommand("DELETE FROM smart_home_read.mt_doc_motionsensor", session.Connection);
            await cmd.ExecuteNonQueryAsync(cancellationToken);

            using (var daemon = session.DocumentStore.BuildProjectionDaemon())
            {
                await daemon.RebuildProjection<MotionSensor>(cancellationToken);
            }
            return Unit.Value;
        }
    }
}
