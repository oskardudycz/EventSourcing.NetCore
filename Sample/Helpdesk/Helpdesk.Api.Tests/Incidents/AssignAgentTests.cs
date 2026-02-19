using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class AssignAgentToIncidentTests(ApiWithLoggedIncident api): IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task AssignAgentCommand_ChangesIncidentCategory() =>
        await api
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{api.Incident.Id}/assign"),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK)
            .And()
            .When(GET, URI($"/api/incidents/{api.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    api.Incident with { AgentId = agentId, Version = 2 }
                )
            );

    private readonly Guid agentId = Guid.CreateVersion7();
}
