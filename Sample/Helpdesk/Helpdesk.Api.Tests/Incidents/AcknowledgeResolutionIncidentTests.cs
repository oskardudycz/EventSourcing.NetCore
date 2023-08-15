using Helpdesk.Api.Incidents;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class AcknowledgeResolutionIncidentTests: IClassFixture<ApiWithResolvedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public  Task ResolveCommand_Succeeds() =>
        API
            .Given()
            .When(
                POST,
                URI($"/api/customers/{API.Incident.CustomerId}/incidents/{API.Incident.Id}/acknowledge"),
                HEADERS(IF_MATCH(2))
            )
            .Then(OK)

            .And()

            .When(GET, URI($"/api/incidents/{API.Incident.Id}"))
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

    private readonly ApiWithResolvedIncident API;

    public AcknowledgeResolutionIncidentTests(ApiWithResolvedIncident api) => API = api;
}
