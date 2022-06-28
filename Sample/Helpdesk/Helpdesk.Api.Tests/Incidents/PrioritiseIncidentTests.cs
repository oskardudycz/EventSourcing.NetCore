using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class PrioritiseIncidentTests: IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task PrioritiseCommand_ChangesIncidentPriority()
    {
        await API
            .Given(
                URI($"/api/agents/{agentId}/incidents/{API.IncidentId}/priority"),
                BODY(new PrioritiseIncidentRequest(priority)),
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
                        Priority = priority,
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly IncidentPriority priority = new Faker().PickRandom<IncidentPriority>();
    private readonly ApiWithLoggedIncident API;

    public PrioritiseIncidentTests(ApiWithLoggedIncident api) => API = api;

}
