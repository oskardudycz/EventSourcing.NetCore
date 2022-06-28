using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithResolvedIncident: ApiWithLoggedIncident
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await Given(
                URI($"/api/agents/{AgentId}/incidents/{IncidentId}/resolve"),
                BODY(new ResolveIncidentRequest(resolutionType)),
                HEADERS(IF_MATCH(1))
            )
            .When(POST)
            .Then(OK);

        Details = new IncidentDetails(
            IncidentId,
            CustomerId,
            IncidentStatus.Resolved,
            Array.Empty<IncidentNote>(),
            null,
            null,
            null,
            2
        );
    }

    public readonly Guid AgentId = Guid.NewGuid();
    private readonly ResolutionType resolutionType = new Faker().PickRandom<ResolutionType>();
}

