using Alba;
using Bogus;
using Bogus.DataSets;
using FluentAssertions;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.Logging;
using Xunit;
using Ogooreck.API;
using Wolverine.Http;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class LogIncidentsTests(AppFixture fixture): IntegrationContext(fixture)
{
    [Fact]
    public async Task LogIncident_ShouldSucceed()
    {
        // POST a CategoriseIncident to the HTTP endpoint, wait until all
        // cascading Wolverine processing is complete
        var result = await  Host.Scenario(x =>
        {
            x.Post.Json(new LogIncident(CustomerId, Contact, IncidentDescription))
                .ToUrl($"/api/customers/{CustomerId}/incidents/");

            x.StatusCodeShouldBe(201);
        });

        var response = await result.ReadAsJsonAsync<CreationResponse>();
        response.Should().NotBeNull();
        response!.Url.Should().StartWith("/api/incidents/");

        result = await Host.Scenario(x =>
        {
            x.Get.Url(response.Url);

            x.StatusCodeShouldBeOk();
        });

        var incident = await result.ReadAsJsonAsync<IncidentDetails>();
        incident.Should().NotBeNull();
        incident!.CustomerId.Should().Be(CustomerId);
        incident.Status.Should().Be(IncidentStatus.Pending);
        incident.Notes.Should().BeEmpty();
        incident.Category.Should().BeNull();
        incident.Priority.Should().BeNull();
        incident.AgentId.Should().BeNull();
        incident.Version.Should().Be(1);
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
