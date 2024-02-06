using Marten;
using Marten.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api.Incidents.GettingDetails;

public static class GetDetailsEndpoints
{
    [WolverineGet("/api/incidents/{incidentId:guid}")]
    public static Task GetIncidentById([FromRoute] Guid incidentId, IQuerySession querySession, HttpContext context) =>
        querySession.Json.WriteById<IncidentDetails>(incidentId, context);
}
