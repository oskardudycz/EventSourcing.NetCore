using Bogus;
using Bogus.DataSets;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.AcknowledgingResolution;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.Logging;
using Helpdesk.Api.Incidents.Resolving;
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

        return await api.Scenario(
            api.ResolveIncident(incident.Id, agentId, resolvedType),
            _ => api.GetIncidentDetails(incident.Id)
        ).GetResponseBody<IncidentDetails>();
    }

    public static async Task<IncidentDetails> AcknowledgedIncident(
        this ApiSpecification<Program> api
    )
    {
        var incident = await api.ResolvedIncident();

        return await api.Scenario(
            api.AcknowledgeIncident(incident.Id, incident.CustomerId),
            _ => api.GetIncidentDetails(incident.Id)
        ).GetResponseBody<IncidentDetails>();
    }

    private static Task<Result> LogIncident(
        this ApiSpecification<Program> api,
        Guid customerId,
        Contact contact,
        string incidentDescription
    ) =>
        api.Given()
            .When(POST, URI($"api/customers/{customerId}/incidents/"), BODY(new LogIncidentRequest(contact, incidentDescription)))
            .Then(CREATED_WITH_DEFAULT_HEADERS(locationHeaderPrefix: "/api/incidents/"));

    private static Task<Result> ResolveIncident<T>(
        this ApiSpecification<T> api,
        Guid incidentId,
        Guid agentId,
        ResolutionType resolutionType
    ) where T : class =>
        api.Given()
            .When(
                POST,
                URI($"/api/agents/{agentId}/incidents/{incidentId}/resolve"),
                BODY(new ResolveIncidentRequest(incidentId, resolutionType)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK);

    private static Task<Result> AcknowledgeIncident<T>(
        this ApiSpecification<T> api,
        Guid incidentId,
        Guid customerId
    ) where T : class =>
        api.Given()
            .When(
                POST,
                URI($"/api/customers/{customerId}/incidents/{incidentId}/acknowledge"),
                BODY(new AcknowledgeResolutionRequest(incidentId)),
                HEADERS(IF_MATCH(2))
             )
            .Then(OK);

    private static Task<Result> GetIncidentDetails(
        this ApiSpecification<Program> api,
        Guid incidentId
    ) =>
        api.Given()
            .When(GET, URI($"/api/incidents/{incidentId}"))
            .Then(OK);
}
