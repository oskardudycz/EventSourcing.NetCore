using Helpdesk.Api.Incidents.RecordingAgentResponse;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;
using Wolverine.Marten;
using static Microsoft.AspNetCore.Http.TypedResults;

namespace Helpdesk.Api.Incidents.RecordingCustomerResponse;

public static class RecordCustomerResponseEndpoint
{
    [AggregateHandler]
    [WolverinePost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/responses/")]
    public static (IResult, Events) RecordCustomerResponse
    (
        RecordAgentResponseToIncidentRequest request,
        Incident incident,
        [FromRoute] Guid customerId,
        [FromRoute] Guid incidentId,
        //TODO: [FromIfMatchHeader] string eTag,
        DateTimeOffset now
    )
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var response = new IncidentResponse.FromCustomer(
            customerId, request.Content
        );

        return (Ok(), [new CustomerRespondedToIncident(incidentId, response, now)]);
    }
}

public record RecordCustomerResponseToIncidentRequest(
    Guid IncidentId,
    string Content
);

