using Bogus.DataSets;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class RecordCustomerResponseToIncidentTests(ApiWithLoggedIncident api): IClassFixture<ApiWithLoggedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task RecordCustomerResponseCommand_RecordsResponse()
    {
        await api
            .Given()
            .When(
                POST,
                URI($"/api/customers/{customerId}/incidents/{api.Incident.Id}/responses"),
                BODY(new RecordCustomerResponseToIncidentRequest(content)),
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
                        Notes =
                        [new IncidentNote(IncidentNoteType.FromCustomer, customerId, content, true)],
                        Version = 2
                    }
                )
            );
    }

    private readonly Guid customerId = Guid.NewGuid();
    private readonly string content = new Lorem().Sentence();
}
