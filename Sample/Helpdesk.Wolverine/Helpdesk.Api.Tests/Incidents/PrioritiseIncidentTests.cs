using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.Prioritising;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class PrioritiseIncidentTests(ApiWithLoggedIncident API):
    IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task PrioritiseCommand_ChangesIncidentPriority()
    {
        await API
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{API.Incident.Id}/priority"),
                BODY(new PrioritiseIncident(API.Incident.Id, agentId, priority)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK);

        await API
            .Given()
            .When(GET, URI($"/api/incidents/{API.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    API.Incident with
                    {
                        Priority = priority,
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly IncidentPriority priority = new Faker().PickRandom<IncidentPriority>();
}
