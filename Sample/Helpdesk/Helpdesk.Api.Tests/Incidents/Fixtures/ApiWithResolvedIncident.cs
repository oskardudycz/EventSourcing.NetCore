using Helpdesk.Api.Incidents.GetIncidentDetails;
using Ogooreck.API;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithResolvedIncident: ApiSpecification<Program>, IAsyncLifetime
{
    public async ValueTask InitializeAsync()
    {
        Incident = await this.ResolvedIncident();
    }

    public IncidentDetails Incident { get; set; } = null!;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

