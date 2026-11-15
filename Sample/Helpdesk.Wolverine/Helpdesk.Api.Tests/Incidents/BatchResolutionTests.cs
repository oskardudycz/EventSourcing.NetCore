using System.Collections.Immutable;
using Alba;
using Shouldly;
using Helpdesk.Api.Incidents;
using Helpdesk.Api.Incidents.ResolutionBatch;
using Helpdesk.Api.Tests.Incidents.Fixtures;
using Xunit;
using Wolverine.Http;
using Wolverine.Tracking;

namespace Helpdesk.Api.Tests.Incidents;

public class BatchResolutionTests(AppFixture fixture): IntegrationContext(fixture)
{
    [Fact]
    public async Task InitiateBatch_ShouldSucceed()
    {
        // Given
        List<Guid> incidents =
        [
            (await Host.LoggedIncident()).Id,
            (await Host.LoggedIncident()).Id,
            (await Host.LoggedIncident()).Id
        ];

        // When
        var result = await Host.Scenario(x =>
        {
            x.Post.Json(new InitiateIncidentsBatchResolution(incidents, agentId, resolution))
                .ToUrl($"/api/agents/{agentId}/incidents/resolve");

            x.StatusCodeShouldBe(201);
        });

        // Then
        // Check the HTTP Response
        var response = await result.ReadAsJsonAsync<CreationResponse>();
        response.ShouldNotBeNull();
        response!.Url.ShouldStartWith("/api/incidents/resolution/");

        // Check if details are available
        result = await Host.Scenario(x =>
        {
            x.Get.Url(response.Url);

            x.StatusCodeShouldBeOk();
        });

        var updated = await result.ReadAsJsonAsync<IncidentsBatchResolution>();
        updated.ShouldBeEquivalentTo(
            new IncidentsBatchResolution(
                response.GetCreatedId("/api/incidents/resolution/"),
                incidents.ToImmutableDictionary(ks => ks, _ => ResolutionStatus.Pending),
                ResolutionStatus.Pending,
                1
            )
        );
    }

    [Fact]
    public async Task Batch_ShouldComplete()
    {
        // Given
        List<Guid> incidents =
        [
            (await Host.LoggedIncident()).Id,
            (await Host.LoggedIncident()).Id,
            (await Host.LoggedIncident()).Id
        ];

        var (session, result) = await TrackedHttpCall(x =>
        {
            x.Post.Json(new InitiateIncidentsBatchResolution(incidents, agentId, resolution))
                .ToUrl($"/api/agents/{agentId}/incidents/resolve");

            x.StatusCodeShouldBe(201);
        });
        var creationResponse = await result.ReadAsJsonAsync<CreationResponse>();
        var batchId = creationResponse!.GetCreatedId("/api/incidents/resolution/");

        // Then
        session.Status.ShouldBe(TrackingStatus.Completed);

        // Check if details are available
        result = await Host.Scenario(x =>
        {
            x.Get.Url($"/api/incidents/resolution/{batchId}");

            x.StatusCodeShouldBeOk();
        });

        var updated = await result.ReadAsJsonAsync<IncidentsBatchResolution>();
        updated.ShouldNotBeNull();
        updated.ShouldBeEquivalentTo(
            new IncidentsBatchResolution(
                batchId,
                incidents.ToImmutableDictionary(ks => ks, _ => ResolutionStatus.Resolved),
                ResolutionStatus.Resolved,
                incidents.Count + 2 // for initiated and completed
            )
        );
    }

    private readonly Guid agentId = Guid.CreateVersion7();
    private readonly ResolutionType resolution = ResolutionType.Permanent;
}
