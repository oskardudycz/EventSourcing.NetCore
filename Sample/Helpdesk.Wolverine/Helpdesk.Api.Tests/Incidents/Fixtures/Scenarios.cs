using Alba;
using Bogus;
using Bogus.DataSets;
using FluentAssertions;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.AcknowledgingResolution;
using Helpdesk.Api.Incidents.GettingDetails;
using Helpdesk.Api.Incidents.Logging;
using Helpdesk.Api.Incidents.Resolving;
using Wolverine.Http;
using Xunit;

namespace Helpdesk.Api.Tests.Incidents.Fixtures;

public static class Scenarios
{
    private static readonly Faker faker = new();
    private static readonly Lorem loremIpsum = new();

    public static async Task<IncidentDetails> LoggedIncident(
        this IAlbaHost api
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

        var result = await api.LogIncident(customerId, contact, incidentDescription);

        result = await api.GetIncidentDetails(await result.GetCreatedId());
        var incident = await result.ReadAsJsonAsync<IncidentDetails>();
        incident.Should().NotBeNull();
        return incident!;
    }

    public static async Task<IncidentDetails> ResolvedIncident(
        this IAlbaHost api
    )
    {
        var agentId = Guid.NewGuid();
        var resolvedType = faker.PickRandom<ResolutionType>();
        var incident = await api.LoggedIncident();

        await api.ResolveIncident(incident.Id, agentId, resolvedType, incident.Version);

        var result = await api.GetIncidentDetails(incident.Id);
        incident = await result.ReadAsJsonAsync<IncidentDetails>();
        incident.Should().NotBeNull();
        return incident!;
    }

    public static async Task<IncidentDetails> AcknowledgedIncident(
        this IAlbaHost api
    )
    {
        var incident = await api.ResolvedIncident();

        await api.AcknowledgeIncident(incident.Id, incident.CustomerId, incident.Version);

        var result = await api.GetIncidentDetails(incident.Id);
        incident = await result.ReadAsJsonAsync<IncidentDetails>();
        incident.Should().NotBeNull();
        return incident!;
    }

    private static Task<IScenarioResult> LogIncident(
        this IAlbaHost api,
        Guid customerId,
        Contact contact,
        string incidentDescription
    ) =>
        api.Scenario(x =>
        {
            x.Post.Url($"/api/customers/{customerId}/incidents/");
            x.Post.Json(new LogIncident(customerId, contact, incidentDescription));

            x.StatusCodeShouldBe(201);
        });

    private static Task<IScenarioResult> ResolveIncident(
        this IAlbaHost api,
        Guid incidentId,
        Guid agentId,
        ResolutionType resolutionType,
        int expectedVersion
    ) =>
        api.Scenario(x =>
        {
            x.Post.Url($"/api/agents/{agentId}/incidents/{incidentId}/resolve");
            x.Post.Json(new ResolveIncident(incidentId, agentId, resolutionType, expectedVersion));

            x.StatusCodeShouldBeOk();
        });

    private static Task<IScenarioResult> AcknowledgeIncident(
        this IAlbaHost api,
        Guid incidentId,
        Guid customerId,
        int expectedVersion
    ) =>
        api.Scenario(x =>
        {
            x.Post.Url($"/api/customers/{customerId}/incidents/{incidentId}/acknowledge");
            x.Post.Json(new AcknowledgeResolution(incidentId, customerId, expectedVersion));

            x.StatusCodeShouldBeOk();
        });

    public static Task<IScenarioResult> GetIncidentDetails(
        this IAlbaHost api,
        Guid incidentId
    ) =>
        api.Scenario(x =>
        {
            x.Get.Url($"/api/incidents/{incidentId}");

            x.StatusCodeShouldBeOk();
        });

    public static async Task<IScenarioResult> IncidentDetailsShouldBe(
        this IAlbaHost api,
        IncidentDetails incident
    )
    {
        var result = await api.GetIncidentDetails(incident.Id);

        var updated = await result.ReadAsJsonAsync<IncidentDetails>();
        updated.Should().BeEquivalentTo(incident);

        return result;
    }

    public static async Task<Guid> GetCreatedId(this IScenarioResult result)
    {
        var response = await result.ReadAsJsonAsync<CreationResponse>();
        response.Should().NotBeNull();
        response!.Url.Should().StartWith("/api/incidents/");

        return response.GetCreatedId();
    }

    public static Guid GetCreatedId(this CreationResponse response)
    {
        response.Url.Should().StartWith("/api/incidents/");

        var createdId = response.Url["/api/incidents/".Length..];

        if (!Guid.TryParse(createdId, out var guid))
        {
            Assert.Fail("Wrong Created Id");
        }

        return guid;
    }
}
