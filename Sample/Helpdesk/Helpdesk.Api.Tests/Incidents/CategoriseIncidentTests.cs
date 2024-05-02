using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class CategoriseIncidentTests(ApiWithLoggedIncident api): IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task CategoriseCommand_ChangesIncidentCategory()
    {
        await api
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{api.Incident.Id}/category"),
                BODY(new CategoriseIncidentRequest(category)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK);

        await api
            .Given()
            .When(GET, URI($"/api/incidents/{api.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    api.Incident with { Category = category, Version = 2 }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly IncidentCategory category = new Faker().PickRandom<IncidentCategory>();
}
