using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithAcknowledgedIncident: ApiWithResolvedIncident
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        await Given(
                URI($"/api/customers/{CustomerId}/incidents/{IncidentId}/acknowledge"),
                HEADERS(IF_MATCH(2))
            )
            .When(POST)
            .Then(OK);

        Details = new IncidentDetails(
            IncidentId,
            CustomerId,
            IncidentStatus.ResolutionAcknowledgedByCustomer,
            Array.Empty<IncidentNote>(),
            null,
            null,
            null,
            3
        );
    }
}

