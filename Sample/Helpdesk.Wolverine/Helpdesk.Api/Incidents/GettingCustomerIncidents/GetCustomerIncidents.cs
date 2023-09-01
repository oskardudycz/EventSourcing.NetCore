using Helpdesk.Api.Incidents.GettingShortInfo;
using Marten;
using Marten.Pagination;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Helpdesk.Api.Incidents.GettingCustomerIncidents;

public static class GetCustomerIncidentsEndpoints
{
    [WolverineGet("/api/customers/{customerId:guid}/incidents/")]
    public static Task<IPagedList<IncidentShortInfo>> GetCustomerIncidents
    (IQuerySession querySession, [FromRoute] Guid customerId, [FromQuery] int? pageNumber, [FromQuery] int? pageSize,
        CancellationToken ct) =>
        querySession.Query<IncidentShortInfo>().Where(i => i.CustomerId == customerId)
            .ToPagedListAsync(pageNumber ?? 1, pageSize ?? 10, ct);
}
