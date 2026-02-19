using Alba;
using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.Prioritising;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class PrioritiseIncidentTests(AppFixture fixture): ApiWithLoggedIncident(fixture)
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task PrioritiseCommand_ChangesIncidentPriority()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new PrioritiseIncident(Incident.Id, agentId, priority, Incident.Version))
                .ToUrl($"/api/agents/{agentId}/incidents/{Incident.Id}/priority");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with { Priority = priority, Version = 2 });
    }

    private readonly Guid agentId = Guid.CreateVersion7();
    private readonly IncidentPriority priority = new Faker().PickRandom<IncidentPriority>();
}
