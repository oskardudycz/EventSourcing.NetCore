using Core.Events;
using Core.Testing;
using FluentAssertions;
using MeetingsSearch.Meetings;
using MeetingsSearch.Meetings.CreatingMeeting;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace MeetingsSearch.IntegrationTests.Meetings.CreatingMeeting;

public class CreateMeetingTests: IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly ApiSpecification<Program> API;
    private readonly TestWebApplicationFactory<Program> fixture;

    public CreateMeetingTests(TestWebApplicationFactory<Program> fixture)
    {
        this.fixture = fixture;
        API = ApiSpecification<Program>.Setup(fixture);
    }

    [Fact(Skip = "Skipped for now until Elastic is fully bumped")]
    [Trait("Category", "Acceptance")]
    public async Task CreateCommand_ShouldPublish_MeetingCreateEvent()
    {
        await fixture.PublishInternalEvent(new EventEnvelope<MeetingCreated>(
            new MeetingCreated(
                MeetingId,
                MeetingName
            ),
            new EventMetadata("event-id", 1, 2, null)
        ));

        await API.Given(
                URI($"{MeetingsSearchApi.MeetingsUrl}?filter={MeetingName}")
            )
            .When(
                GET_UNTIL(
                    RESPONSE_BODY_MATCHES<IReadOnlyCollection<Meeting>>(
                        meetings => meetings.Any(m => m.Id == MeetingId))
                ))
            .Then(
                RESPONSE_BODY<IReadOnlyCollection<Meeting>>(meetings =>
                    meetings.Should().Contain(meeting =>
                        meeting.Id == MeetingId
                        && meeting.Name == MeetingName
                    )
                ));
    }

    private readonly Guid MeetingId = Guid.NewGuid();
    private readonly string MeetingName = "Event Sourcing Workshop";
}
