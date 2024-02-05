using Bogus;
using Bogus.DataSets;
using FluentAssertions;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.Logging;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using Wolverine.Http;

namespace Helpdesk.Api.Tests.Incidents;

public class LogIncidentsTests(AppFixture fixture): IntegrationContext(fixture)
{
    [Fact]
    public async Task LogIncident_ShouldSucceed()
    {
        var result = await Host.Scenario(x =>
        {
            x.Post.Json(new LogIncident(CustomerId, Contact, IncidentDescription))
                .ToUrl($"/api/customers/{CustomerId}/incidents/");

            x.StatusCodeShouldBe(201);
        });

        var response = await result.ReadAsJsonAsync<CreationResponse>();
        response.Should().NotBeNull();
        response!.Url.Should().StartWith("/api/incidents/");


        await Host.IncidentDetailsShouldBe(
            new IncidentDetails(
                response.GetCreatedId(),
                CustomerId,
                IncidentStatus.Pending,
                [],
                null,
                null,
                null,
                1
            )
        );
    }

    private readonly Guid CustomerId = Guid.NewGuid();

    private readonly Contact Contact = new Faker<Contact>().CustomInstantiator(
        f => new Contact(
            f.PickRandom<ContactChannel>(),
            f.Name.FirstName(),
            f.Name.LastName(),
            f.Internet.Email(),
            f.Phone.PhoneNumber()
        )
    ).Generate();

    private readonly string IncidentDescription = new Lorem().Sentence();
}
