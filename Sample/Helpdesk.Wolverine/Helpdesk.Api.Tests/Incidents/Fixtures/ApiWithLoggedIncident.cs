using Helpdesk.Api.Incidents.GettingDetails;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithLoggedIncident(AppFixture fixture): IntegrationContext(fixture), IAsyncLifetime
{
    public override async Task InitializeAsync() =>
        Incident = await Host.LoggedIncident();

    public IncidentDetails Incident { get; protected set; } = null!;
}
