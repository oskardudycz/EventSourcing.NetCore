using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api.Incidents.GettingDetails;

public static class GetDetailsEndpoints
{
    // That for some reason doesn't work for me
    // [WolverineGet("/api/incidents/{incidentId:guid}")]
    // public static Task GetIncidentById([FromRoute] Guid incidentId, IQuerySession querySession, HttpContext context) =>
    //     querySession.Json.WriteById<IncidentDetails>(incidentId, context);

    [WolverineGet("/api/incidents/{incidentId:guid}")]
    public static Task<IncidentDetails?> GetDetails([FromRoute] Guid incidentId, IQuerySession querySession,
        CancellationToken ct) =>
        querySession.LoadAsync<IncidentDetails>(incidentId, ct);
}
