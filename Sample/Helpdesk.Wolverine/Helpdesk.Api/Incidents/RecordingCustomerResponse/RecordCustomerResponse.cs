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
        RecordCustomerResponseToIncident command,
        Incident incident,
        DateTimeOffset now
    )
    {
        if (incident.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (_, customerId, content) = command;

        var response = new IncidentResponse.FromCustomer(
            customerId, content
        );

        return (Ok(), [new CustomerRespondedToIncident(incident.Id, response, now)]);
    }
}

public record RecordCustomerResponseToIncident(
    Guid IncidentId,
    Guid CustomerId,
    string Content
);
