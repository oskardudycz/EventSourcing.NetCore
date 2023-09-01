using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api.Incidents.GettingHistory;

public static class GetHistoryEndpoint
{
    //That for some reason doesn't work for me
    // [WolverineGet("/api/incidents/{incidentId:guid}/history")]
    // public static Task GetIncidentHistory([FromRoute]Guid incidentId, HttpContext context, IQuerySession querySession) =>
    //     querySession.Query<IncidentHistory>().Where(i => i.IncidentId == incidentId).WriteArray(context);

    [WolverineGet("/api/incidents/{incidentId:guid}/history")]
    public static Task<IReadOnlyList<IncidentHistory>> GetHistory(
        [FromRoute] Guid incidentId,
        IQuerySession querySession,
        CancellationToken ct
    ) =>
        querySession.Query<IncidentHistory>().Where(i => i.IncidentId == incidentId).ToListAsync(ct);
}
