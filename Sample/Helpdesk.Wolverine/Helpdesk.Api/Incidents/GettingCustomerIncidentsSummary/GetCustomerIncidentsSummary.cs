using Marten;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api.Incidents.GettingCustomerIncidentsSummary;

public static class GetCustomerIncidentsSummaryEndpoint
{
    // That for some reason doesn't work for me
    // [WolverineGet("/api/customers/{customerId:guid}/incidents/incidents-summary")]
    // public static Task GetCustomerIncidentsSummary([FromRoute] Guid customerId, HttpContext context,
    //     IQuerySession querySession) =>
    //     querySession.Json.WriteById<CustomerIncidentsSummary>(customerId, context);

    [WolverineGet("/api/customers/{customerId:guid}/incidents/incidents-summary")]
    public static Task<CustomerIncidentsSummary?> GetCustomerIncidentsSummary(
        [FromRoute] Guid customerId,
        IQuerySession querySession,
        CancellationToken ct
    ) =>
        querySession.LoadAsync<CustomerIncidentsSummary>(customerId, ct);

}
