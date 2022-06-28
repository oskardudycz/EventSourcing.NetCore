using Bogus;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public class ApiWithLoggedIncident: ApiSpecification<Program>, IAsyncLifetime
{
    public virtual async Task InitializeAsync()
    {
        var response = await Given(
                URI($"api/customers/{CustomerId}/incidents/"),
                BODY(new LogIncidentRequest(Contact, IncidentDescription))
            )
            .When(POST)
            .Then(CREATED_WITH_DEFAULT_HEADERS(locationHeaderPrefix: "/api/incidents/"));

        IncidentId = response.GetCreatedId<Guid>();

        Details = new IncidentDetails(
            IncidentId,
            CustomerId,
            IncidentStatus.Pending,
            Array.Empty<IncidentNote>(),
            null,
            null,
            null,
            1
        );
    }

    public Guid IncidentId { get; set; }

    public IncidentDetails Details { get; protected set; } = default!;

    public readonly Guid CustomerId = Guid.NewGuid();

    private readonly Contact Contact = new Faker<Contact>().CustomInstantiator(
        f => new Contact(
            f.PickRandom<ContactChannel>(),
            f.Name.FirstName(),
            f.Name.LastName(),
            f.Internet.Email(),
            f.Phone.PhoneNumber()
        )
    ).Generate();

    private readonly string IncidentDescription = new Bogus.DataSets.Lorem().Sentence();

    public Task DisposeAsync() => Task.CompletedTask;
}
