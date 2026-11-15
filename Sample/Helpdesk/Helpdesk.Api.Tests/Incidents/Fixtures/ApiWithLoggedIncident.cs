using Helpdesk.Api.Incidents.GetIncidentDetails;
using Ogooreck.API;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithLoggedIncident: ApiSpecification<Program>, IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        Incident = await this.LoggedIncident();
    }
    public IncidentDetails Incident { get; protected set; } = null!;
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
