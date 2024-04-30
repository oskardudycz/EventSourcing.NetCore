using Bogus;
using Bogus.DataSets;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class RecordAgentResponseToIncidentTests: IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task RecordAgentResponseCommand_RecordsResponse()
    {
        await API
            .Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{API.Incident.Id}/responses"),
                BODY(new RecordAgentResponseToIncidentRequest(content, visibleToCustomer)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK);

        await API
            .Given()
            .When(GET, URI($"/api/incidents/{API.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    API.Incident with
                    {
                        Notes =
                        [
                            new IncidentNote(IncidentNoteType.FromAgent, agentId, content, visibleToCustomer)
                        ],
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly string content = new Lorem().Sentence();
    private readonly bool visibleToCustomer = new Faker().Random.Bool();
    private readonly ApiWithLoggedIncident API;

    public RecordAgentResponseToIncidentTests(ApiWithLoggedIncident api) => API = api;
}
