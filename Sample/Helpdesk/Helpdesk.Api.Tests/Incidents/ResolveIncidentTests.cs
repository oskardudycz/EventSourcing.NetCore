using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class ResolveIncidentTests(ApiWithLoggedIncident api): IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await api
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{api.Incident.Id}/resolve"),
                BODY(new ResolveIncidentRequest(resolutionType)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK);

        await api
            .Given()
            .When(
                GET,
                URI($"/api/incidents/{api.Incident.Id}")
            )
            .Then(
                OK,
                RESPONSE_BODY(
                    api.Incident with { Status = IncidentStatus.Resolved, Version = 2 }
                )
            );
    }

    private readonly Guid agentId = Guid.CreateVersion7();
    private readonly ResolutionType resolutionType = new Faker().PickRandom<ResolutionType>();
}
