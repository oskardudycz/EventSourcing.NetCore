using Alba;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.AcknowledgingResolution;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class AcknowledgeResolutionIncidentTests(AppFixture fixture): ApiWithResolvedIncident(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new AcknowledgeResolution(Incident.Id, Incident.CustomerId, Incident.Version))
                .ToUrl($"/api/customers/{Incident.CustomerId}/incidents/{Incident.Id}/acknowledge");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with
        {
            Status = IncidentStatus.ResolutionAcknowledgedByCustomer, Version = 3
        });
    }
}
