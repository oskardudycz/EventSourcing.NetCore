using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class AssignAgentToIncidentTests: IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task AssignAgentCommand_ChangesIncidentCategory()
    {
        await API
            .Given(
                URI($"/api/agents/{agentId}/incidents/{API.IncidentId}/assign"),
                HEADERS(IF_MATCH(1))
            )
            .When(POST)
            .Then(OK);

        await API
            .Given(URI($"/api/incidents/{API.IncidentId}"))
            .When(GET)
            .Then(
                OK,
                RESPONSE_BODY(
                    API.Details with
                    {
                        AgentId = agentId,
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly ApiWithLoggedIncident API;

    public AssignAgentToIncidentTests(ApiWithLoggedIncident api) => API = api;
}
