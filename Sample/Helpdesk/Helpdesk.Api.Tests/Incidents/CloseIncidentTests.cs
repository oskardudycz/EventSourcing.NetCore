using Helpdesk.Api.Incidents;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class CloseIncidentTests(ApiWithAcknowledgedIncident api): IClassFixture<ApiWithAcknowledgedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await api
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{api.Incident.Id}/close"),
                HEADERS(IF_MATCH(3))
            )
            .Then(OK);

        await api
            .Given()
            .When(GET, URI($"/api/incidents/{api.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    api.Incident with { Status = IncidentStatus.Closed, Version = 4 }
                )
            );
    }

    private Guid agentId = Guid.NewGuid();
}
