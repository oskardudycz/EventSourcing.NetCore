using Alba;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.Closing;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class CloseIncidentTests(AppFixture fixture): ApiWithAcknowledgedIncident(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new CloseIncident(Incident.Id, Incident.Version))
                .ToUrl($"/api/agents/{agentId}/incidents/{Incident.Id}/close");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with { Status = IncidentStatus.Closed, Version = 4 });
    }

    private Guid agentId = Guid.CreateVersion7();
}
