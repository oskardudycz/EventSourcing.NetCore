using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api.Incidents.ResolutionBatch.GettingIncidentResolutionBatch;

public class GetIncidentsBatchResolution
{
    [WolverineGet("/api/incidents/resolution/{batchId:guid}")]
    public static Task<IncidentsBatchResolution?> Get([FromRoute] Guid batchId, [FromQuery]long version, IQuerySession querySession,
        CancellationToken ct) =>
        querySession.Events.AggregateStreamAsync<IncidentsBatchResolution>(batchId, version: version, token: ct);
}
