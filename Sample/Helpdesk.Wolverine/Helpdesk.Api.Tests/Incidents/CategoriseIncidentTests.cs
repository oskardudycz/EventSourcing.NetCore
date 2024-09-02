using Alba;
using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.Categorising;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class CategoriseIncidentTests(AppFixture fixture): ApiWithLoggedIncident(fixture)
{
    [Fact(Skip = "Need to bump wolverine")]
    [Trait("Category", "Acceptance")]
    public async Task CategoriseCommand_ChangesIncidentCategory()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new CategoriseIncident(Incident.Id, category, Incident.Version))
                .ToUrl($"/api/agents/{agentId}/incidents/{Incident.Id}/category");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with { Category = category, Version = 2 });
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly IncidentCategory category = new Faker().PickRandom<IncidentCategory>();
}
