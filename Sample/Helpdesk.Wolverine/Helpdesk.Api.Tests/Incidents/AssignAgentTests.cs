using Helpdesk.Api.Incidents.AssigningAgent;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class AssignAgentToIncidentTests(ApiWithLoggedIncident API):
    IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task AssignAgentCommand_ChangesIncidentCategory()
    {
        await API
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{API.Incident.Id}/assign"),
                HEADERS(IF_MATCH(1)),
                BODY(new AssignAgentToIncident(API.Incident.Id))
            )
            .Then(OK)
            .And()
            .When(GET, URI($"/api/incidents/{API.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    API.Incident with { AgentId = agentId, Version = 2 }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
}
