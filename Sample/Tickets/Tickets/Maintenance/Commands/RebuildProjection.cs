using Ardalis.GuardClauses;
using Core.Commands;

namespace Tickets.Maintenance.Commands
{
    public class RebuildProjection : ICommand
    {
        public string ProjectionName { get; }

        private RebuildProjection(string projectionName)
        {
            ProjectionName = projectionName;
        }


        public static RebuildProjection Create(string projectionName)
        {
            Guard.Against.NullOrEmpty(projectionName, nameof(projectionName));

            return new RebuildProjection(projectionName);
        }
    }
}
