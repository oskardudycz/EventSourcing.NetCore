using Helpdesk.Api.Core.Http;
using Helpdesk.Api.Core.Marten;
using Marten;
using Wolverine.Http;
using static Microsoft.AspNetCore.Http.TypedResults;
using static Helpdesk.Api.Core.Http.ETagExtensions;
using static System.DateTimeOffset;

namespace Helpdesk.Api.Incidents.RecordingCustomerResponse;

public static class RecordCustomerResponseEndpoint
{
    [WolverinePost("/api/customers/{customerId:guid}/incidents/{incidentId:guid}/responses/")]
    public static async Task<IResult> RecordCustomerResponse
    (
        IDocumentSession documentSession,
        Guid incidentId,
        Guid customerId,
        [FromIfMatchHeader] string eTag,
        RecordCustomerResponseToIncidentRequest body,
        CancellationToken ct
    )
    {
        await documentSession.GetAndUpdate<Incident>(incidentId, ToExpectedVersion(eTag),
            state => Handle(state,
                new RecordCustomerResponseToIncident(incidentId,
                    new IncidentResponse.FromCustomer(customerId, body.Content), Now)), ct);

        return Ok();
    }

    public static CustomerRespondedToIncident Handle(
        Incident current,
        RecordCustomerResponseToIncident command
    )
    {
        if (current.Status == IncidentStatus.Closed)
            throw new InvalidOperationException("Incident is already closed");

        var (incidentId, response, now) = command;

        return new CustomerRespondedToIncident(incidentId, response, now);
    }
}

public record RecordCustomerResponseToIncidentRequest(
    string Content
);

public record RecordCustomerResponseToIncident(
    Guid IncidentId,
    IncidentResponse.FromCustomer Response,
    DateTimeOffset Now
);

