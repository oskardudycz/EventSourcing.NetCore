using Core.Testing;
using MeetingsManagement.Meetings.CreatingMeeting;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace MeetingsManagement.IntegrationTests.Meetings.CreatingMeeting;

public class CreateMeetingTests(TestWebApplicationFactory<Program> fixture)
    : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly ApiSpecification<Program> API = ApiSpecification<Program>.Setup(fixture);
    private readonly TestWebApplicationFactory<Program> fixture = fixture;

    [Fact]
    [Trait("Category", "Acceptance")]
    public Task CreateCommand_ShouldPublish_MeetingCreateEvent() =>
        API.Given()
            .When(
                POST,
                URI(MeetingsManagementApi.MeetingsUrl),
                BODY(new CreateMeeting(MeetingId, MeetingName))
            )
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1));

    private readonly Guid MeetingId = Guid.NewGuid();
    private readonly string MeetingName = "Event Sourcing Workshop";
}
