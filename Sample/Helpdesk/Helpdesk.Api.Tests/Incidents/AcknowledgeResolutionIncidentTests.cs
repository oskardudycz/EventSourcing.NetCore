using Helpdesk.Api.Incidents;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class AcknowledgeResolutionIncidentTests(ApiWithResolvedIncident api): IClassFixture<ApiWithResolvedIncident>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public  Task ResolveCommand_Succeeds() =>
        api
            .Given()
            .When(
                POST,
                URI($"/api/customers/{api.Incident.CustomerId}/incidents/{api.Incident.Id}/acknowledge"),
                HEADERS(IF_MATCH(2))
            )
            .Then(OK)

            .And()

            .When(GET, URI($"/api/incidents/{api.Incident.Id}"))
            .Then(
                OK,
                RESPONSE_BODY(
                    api.Incident with
                    {
                        Status = IncidentStatus.ResolutionAcknowledgedByCustomer,
                        Version = 3
                    }
                )
            );
}
