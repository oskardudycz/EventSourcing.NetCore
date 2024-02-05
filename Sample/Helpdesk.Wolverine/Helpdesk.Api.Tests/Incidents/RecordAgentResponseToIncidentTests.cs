using Alba;
using Bogus;
using Bogus.DataSets;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.RecordingAgentResponse;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class RecordAgentResponseToIncidentTests(AppFixture fixture): ApiWithLoggedIncident(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task RecordAgentResponseCommand_RecordsResponse()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new RecordAgentResponseToIncident(Incident.Id, agentId, content, visibleToCustomer, Incident.Version))
                .ToUrl($"/api/agents/{agentId}/incidents/{Incident.Id}/responses");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with
        {
            Notes =
            [
                new IncidentNote(IncidentNoteType.FromAgent, agentId, content, visibleToCustomer)
            ],
            Version = 2
        });
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly string content = new Lorem().Sentence();
    private readonly bool visibleToCustomer = new Faker().Random.Bool();
}
