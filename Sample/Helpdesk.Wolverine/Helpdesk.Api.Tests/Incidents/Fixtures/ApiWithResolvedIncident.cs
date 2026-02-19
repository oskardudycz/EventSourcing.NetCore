using Helpdesk.Api.Incidents.GettingDetails;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithResolvedIncident(AppFixture fixture): IntegrationContext(fixture), IAsyncLifetime
{
    public override async Task InitializeAsync() =>
        Incident = await Host.ResolvedIncident();

    public IncidentDetails Incident { get; set; } = null!;
}

