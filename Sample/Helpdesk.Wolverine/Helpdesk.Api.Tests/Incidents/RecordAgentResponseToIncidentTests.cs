using Bogus;
using Bogus.DataSets;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.RecordingAgentResponse;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class RecordAgentResponseToIncidentTests(ApiWithLoggedIncident api):
    IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task RecordAgentResponseCommand_RecordsResponse()
    {
        await api
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{api.Incident.Id}/responses"),
                BODY(new RecordAgentResponseToIncident(api.Incident.Id, agentId, content, visibleToCustomer)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK);

        await api
            .Given()
            .When(GET, URI($"/api/incidents/{api.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    api.Incident with
                    {
                        Notes = new[]
                        {
                            new IncidentNote(IncidentNoteType.FromAgent, agentId, content, visibleToCustomer)
                        },
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly string content = new Lorem().Sentence();
    private readonly bool visibleToCustomer = new Faker().Random.Bool();
}
