using Helpdesk.Api.Incidents.GettingDetails;
using Ogooreck.API;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithLoggedIncident: ApiSpecification<Program>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        Incident = await this.LoggedIncident();
    }
    public IncidentDetails Incident { get; protected set; } = default!;
    public Task DisposeAsync() => Task.CompletedTask;
}
