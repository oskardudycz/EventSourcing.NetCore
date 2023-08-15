using Bogus;
using Bogus.DataSets;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents;

public class LogIncidentsTests: IClassFixture<ApiSpecification<Program>>
{
    [Fact]
    public Task LogIncident_ShouldSucceed() =>
        API.Given()
            .When(
                POST,
                URI($"api/customers/{CustomerId}/incidents/"),
                BODY(new LogIncidentRequest(Contact, IncidentDescription))
            )
            .Then(CREATED_WITH_DEFAULT_HEADERS(locationHeaderPrefix: "/api/incidents/"))
            .And()
            .When(GET, URI(ctx => $"/api/incidents/{ctx.GetCreatedId()}"))
            .Then(
                OK,
                RESPONSE_BODY(ctx =>
                    new IncidentDetails(
                        ctx.GetCreatedId<Guid>(),
                        CustomerId,
                        IncidentStatus.Pending,
                        Array.Empty<IncidentNote>(),
                        null,
                        null,
                        null,
                        1
                    )
                )
            );

    public LogIncidentsTests(ApiSpecification<Program> api) => API = api;

    private readonly ApiSpecification<Program> API;

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
