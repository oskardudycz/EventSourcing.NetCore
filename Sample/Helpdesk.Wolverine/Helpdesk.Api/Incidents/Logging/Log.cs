using Wolverine.Http;
using Wolverine.Marten;

namespace Helpdesk.Api.Incidents.Logging;

public static class LogEndpoint
{
    [WolverinePost("/api/customers/{customerId:guid}/incidents")]
    public static (CreationResponse, IStartStream) LogIncident(
        LogIncident command,
        DateTimeOffset now
    )
    {
        var (customerId, contact, description) = command;
        var incidentId = Guid.CreateVersion7();

        var @event = new IncidentLogged(incidentId, customerId, contact, description, customerId, now);

        return (
            new CreationResponse($"/api/incidents/{incidentId}"),
            new StartStream<Incident>(incidentId, @event)
        );
    }
}

public record LogIncident(
    Guid CustomerId,
    Contact Contact,
    string Description
);
