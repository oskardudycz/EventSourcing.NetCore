using Bogus;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Helpdesk.Incidents;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class ResolveIncidentTests: IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await API
            .Given(
                URI($"/api/agents/{agentId}/incidents/{API.Incident.Id}/resolve"),
                BODY(new ResolveIncidentRequest(resolutionType)),
                HEADERS(IF_MATCH(1))
            )
            .When(POST)
            .Then(OK);

        await API
            .Given(URI($"/api/incidents/{API.Incident.Id}"))
            .When(GET)
            .Then(
                OK,
                RESPONSE_BODY(
                    API.Incident with
                    {
                        Status = IncidentStatus.Resolved,
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly ResolutionType resolutionType = new Faker().PickRandom<ResolutionType>();
    private readonly ApiWithLoggedIncident API;

    public ResolveIncidentTests(ApiWithLoggedIncident api) => API = api;
}
