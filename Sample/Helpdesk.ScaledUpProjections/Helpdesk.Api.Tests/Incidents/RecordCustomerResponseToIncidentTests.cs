using Bogus.DataSets;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Helpdesk.Incidents.GetIncidentDetails;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class RecordCustomerResponseToIncidentTests: IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task RecordCustomerResponseCommand_RecordsResponse()
    {
        await API
            .Given(
                URI($"/api/customers/{customerId}/incidents/{API.Incident.Id}/responses"),
                BODY(new RecordCustomerResponseToIncidentRequest(content)),
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
                        Notes = new[] { new IncidentNote(IncidentNoteType.FromCustomer, customerId, content, true) },
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid customerId = Guid.NewGuid();
    private readonly string content = new Lorem().Sentence();
    private readonly ApiWithLoggedIncident API;

    public RecordCustomerResponseToIncidentTests(ApiWithLoggedIncident api) => API = api;
}
