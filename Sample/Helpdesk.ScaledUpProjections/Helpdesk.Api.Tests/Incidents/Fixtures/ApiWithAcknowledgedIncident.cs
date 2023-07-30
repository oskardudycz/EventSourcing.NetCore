using Helpdesk.Api.Incidents.GetIncidentDetails;
using Ogooreck.API;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithAcknowledgedIncident: ApiSpecification<Program>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        Incident = await this.AcknowledgedIncident();
    }

    public IncidentDetails Incident { get; set; } = default!;

    public Task DisposeAsync() => Task.CompletedTask;
}

