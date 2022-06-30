using Helpdesk.Api.Incidents;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class CloseIncidentTests: IClassFixture<ApiWithAcknowledgedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await API
            .Given(
                URI($"/api/agents/{agentId}/incidents/{API.Incident.Id}/close"),
                HEADERS(IF_MATCH(3))
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
                        Status = IncidentStatus.Closed,
                        Version = 4
                    }
                )
            );
    }

    private readonly ApiWithAcknowledgedIncident API;
    private Guid agentId = Guid.NewGuid();

    public CloseIncidentTests(ApiWithAcknowledgedIncident api) => API = api;
}
