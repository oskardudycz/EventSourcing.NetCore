using Alba;
using Helpdesk.Api.Incidents.AssigningAgent;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class AssignAgentToIncidentTests(AppFixture fixture): ApiWithLoggedIncident(fixture)
{
    [Fact(Skip = "Need to bump wolverine")]
    [Trait("Category", "Acceptance")]
    public async Task AssignAgentCommand_ChangesIncidentCategory()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new AssignAgentToIncident(Incident.Id, Incident.Version))
                .ToUrl($"/api/agents/{agentId}/incidents/{Incident.Id}/assign");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with { AgentId = agentId, Version = 2 });
    }

    private readonly Guid agentId = Guid.NewGuid();
}
