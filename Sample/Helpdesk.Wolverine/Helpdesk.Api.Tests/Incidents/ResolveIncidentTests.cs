using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.Resolving;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class ResolveIncidentTests(ApiWithLoggedIncident API):
    IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await API
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{API.Incident.Id}/resolve"),
                BODY(new ResolveIncident(API.Incident.Id, agentId, resolutionType)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK);

        await API
            .Given()
            .When(
                GET,
                URI($"/api/incidents/{API.Incident.Id}")
            )
            .Then(
                OK,
                RESPONSE_BODY(
                    API.Incident with { Status = IncidentStatus.Resolved, Version = 2 }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly ResolutionType resolutionType = new Faker().PickRandom<ResolutionType>();
}
