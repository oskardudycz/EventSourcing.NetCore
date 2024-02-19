using Helpdesk.Api.Tests.Incidents.Fixtures;
using Helpdesk.Incidents;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class AcknowledgeResolutionIncidentTests: IClassFixture<ApiWithResolvedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await API
            .Given(
                URI($"/api/customers/{API.Incident.CustomerId}/incidents/{API.Incident.Id}/acknowledge"),
                HEADERS(IF_MATCH(2))
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
                        Status = IncidentStatus.ResolutionAcknowledgedByCustomer,
                        Version = 3
                    }
                )
            );
    }

    private readonly ApiWithResolvedIncident API;

    public AcknowledgeResolutionIncidentTests(ApiWithResolvedIncident api) => API = api;
}
