using Bogus;
using Bogus.DataSets;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.GetIncidentDetails;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public static class Scenarios
{
    private static readonly Faker faker = new();
    private static readonly Lorem loremIpsum = new();

    public static async Task<IncidentDetails> LoggedIncident(
        this ApiSpecification<Program> api
    )
    {
        var customerId = Guid.NewGuid();

        var contact = new Contact(
            faker.PickRandom<ContactChannel>(),
            faker.Name.FirstName(),
            faker.Name.LastName(),
            faker.Internet.Email(),
            faker.Phone.PhoneNumber()
        );
        var incidentDescription = loremIpsum.Sentence();

        var response = await api.Scenario(
            api.LogIncident(customerId, contact, incidentDescription),
            r => api.GetIncidentDetails(r.GetCreatedId<Guid>())
        );

        return await response.GetResultFromJson<IncidentDetails>();
    }

    public static async Task<IncidentDetails> ResolvedIncident(
        this ApiSpecification<Program> api
    )
    {
        var agentId = Guid.NewGuid();
        var resolvedType = faker.PickRandom<ResolutionType>();
        var incident = await api.LoggedIncident();

        var response = await api.Scenario(
            api.ResolveIncident(incident.Id, agentId, resolvedType),
            _ => api.GetIncidentDetails(incident.Id)
        );

        return await response.GetResultFromJson<IncidentDetails>();
    }

    public static async Task<IncidentDetails> AcknowledgedIncident(
        this ApiSpecification<Program> api
    )
    {
        var incident = await api.ResolvedIncident();

        var response = await api.Scenario(
            api.AcknowledgeIncident(incident.Id, incident.CustomerId),
            _ => api.GetIncidentDetails(incident.Id)
        );

        return await response.GetResultFromJson<IncidentDetails>();
    }

    private static Task<HttpResponseMessage> LogIncident(
        this ApiSpecification<Program> api,
        Guid customerId,
        Contact contact,
        string incidentDescription
    ) =>
        api.Given(
                URI($"api/customers/{customerId}/incidents/"),
                BODY(new LogIncidentRequest(contact, incidentDescription))
            )
            .When(POST)
            .Then(CREATED_WITH_DEFAULT_HEADERS(locationHeaderPrefix: "/api/incidents/"));

    private static Task<HttpResponseMessage> ResolveIncident<T>(
        this ApiSpecification<T> api,
        Guid incidentId,
        Guid agentId,
        ResolutionType resolutionType
    ) where T : class =>
        api.Given(
                URI($"/api/agents/{agentId}/incidents/{incidentId}/resolve"),
                BODY(new ResolveIncidentRequest(resolutionType)),
                HEADERS(IF_MATCH(1))
            )
            .When(POST)
            .Then(OK);

    private static Task<HttpResponseMessage> AcknowledgeIncident<T>(
        this ApiSpecification<T> api,
        Guid incidentId,
        Guid customerId
    ) where T : class =>
        api.Given(
                URI($"/api/customers/{customerId}/incidents/{incidentId}/acknowledge"),
                HEADERS(IF_MATCH(2))
            )
            .When(POST)
            .Then(OK);

    private static Task<HttpResponseMessage> GetIncidentDetails(
        this ApiSpecification<Program> api,
        Guid incidentId
    ) =>
        api.Given(URI($"/api/incidents/{incidentId}"))
            .When(GET)
            .Then(OK);
}
