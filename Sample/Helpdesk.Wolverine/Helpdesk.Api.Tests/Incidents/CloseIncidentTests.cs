using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.Closing;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class CloseIncidentTests(ApiWithAcknowledgedIncident API):
    IClassFixture<ApiWithAcknowledgedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await API
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{API.Incident.Id}/close"),
                BODY(new CloseIncidentRequest(API.Incident.Id)),
                HEADERS(IF_MATCH(3))
            )
            .Then(OK);

        await API
            .Given()
            .When(GET, URI($"/api/incidents/{API.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    API.Incident with { Status = IncidentStatus.Closed, Version = 4 }
                )
            );
    }

    private Guid agentId = Guid.NewGuid();
}
