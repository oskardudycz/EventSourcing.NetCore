using Alba;
using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.Resolving;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents;

public class ResolveIncidentTests(AppFixture fixture): ApiWithLoggedIncident(fixture)
{
    [Fact(Skip = "Need to bump wolverine")]
    [Trait("Category", "Acceptance")]
    public async Task ResolveCommand_Succeeds()
    {
        await Host.Scenario(x =>
        {
            x.Post.Json(new ResolveIncident(Incident.Id, agentId, resolutionType, Incident.Version))
                .ToUrl($"/api/agents/{agentId}/incidents/{Incident.Id}/resolve");

            x.StatusCodeShouldBeOk();
        });

        await Host.IncidentDetailsShouldBe(Incident with { Status = IncidentStatus.Resolved, Version = 2 });
    }
    [Fact(Skip = "Need to bump wolverine")]
    [Trait("Category", "Acceptance")]
    public async Task ResolvingTwiceTheSame_Incident_Succeeds()
    {
        var version = Incident.Version;
        Action<Scenario> resolve = x =>
        {
            x.Post.Json(new ResolveIncident(Incident.Id, agentId, resolutionType, version))
                .ToUrl($"/api/agents/{agentId}/incidents/{Incident.Id}/resolve");

            x.StatusCodeShouldBeOk();
        };
        await Host.Scenario(resolve);
        version++;

        await Host.Scenario(resolve);
        await Host.IncidentDetailsShouldBe(Incident with { Status = IncidentStatus.Resolved, Version = 2 });
    }

    private readonly Guid agentId = Guid.NewGuid();
    private readonly ResolutionType resolutionType = new Faker().PickRandom<ResolutionType>();
}
