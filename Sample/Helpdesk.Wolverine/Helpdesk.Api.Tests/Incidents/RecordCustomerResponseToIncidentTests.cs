using Alba;
using Bogus.DataSets;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.RecordingCustomerResponse;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class RecordCustomerResponseToIncidentTests(AppFixture fixture): ApiWithLoggedIncident(fixture)
{
    [Fact(Skip = "Need to bump wolverine")]
    [Trait("Category", "Acceptance")]
    public async Task RecordCustomerResponseCommand_RecordsResponse()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new RecordCustomerResponseToIncident(Incident.Id, customerId, content, Incident.Version))
                .ToUrl($"/api/customers/{customerId}/incidents/{Incident.Id}/responses");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with
        {
            Notes =
            [new IncidentNote(IncidentNoteType.FromCustomer, customerId, content, true)],
            Version = 2
        });
    }

    private readonly Guid customerId = Guid.NewGuid();
    private readonly string content = new Lorem().Sentence();
}
