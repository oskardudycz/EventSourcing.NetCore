using Marten;
using Marten.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api.Incidents.GettingCustomerIncidentsSummary;

public static class GetCustomerIncidentsSummaryEndpoint
{
    [WolverineGet("/api/customers/{customerId:guid}/incidents/incidents-summary")]
    public static Task GetCustomerIncidentsSummary([FromRoute] Guid customerId, HttpContext context,
        IQuerySession querySession) =>
        querySession.Json.WriteById<CustomerIncidentsSummary>(customerId, context);
}
