using JasperFx.Core;
using Wolverine.Http;
using Wolverine.Marten;

namespace Helpdesk.Api.Incidents.Logging;

public static class LogEndpoint
{
    [WolverinePost("/api/customers/{customerId:guid}/incidents")]
    public static (CreationResponse, IStartStream) LogIncident(
        Guid customerId,
        LogIncidentRequest request,
        DateTimeOffset now
    )
    {
        var (contact, description) = request;
        var incidentId = CombGuidIdGeneration.NewGuid();

        var @event = new IncidentLogged(incidentId, customerId, contact, description, customerId, now);

        return (
            new CreationResponse($"/api/incidents/{incidentId}"),
            new StartStream<Incident>(incidentId, @event)
        );
    }
}

public record LogIncidentRequest(
    Contact Contact,
    string Description
);
