using FluentAssertions;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.ValueObjects;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace MeetingsManagement.IntegrationTests.Meetings.SchedulingMeetings;


public class ScheduleMeetingFixture: ApiSpecification<Program>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        var openResponse = await Send(
            new ApiRequest(POST, URI(MeetingsManagementApi.MeetingsUrl), BODY(new CreateMeeting(MeetingId, MeetingName)))
        );

        await CREATED_WITH_DEFAULT_HEADERS(eTag: 1)(openResponse);
    }

    public Task DisposeAsync() => Task.CompletedTask;


    public readonly Guid MeetingId = Guid.NewGuid();
    public readonly string MeetingName = "Event Sourcing Workshop";
}

public class ScheduleMeetingTests: IClassFixture<ScheduleMeetingFixture>
{

    private readonly ScheduleMeetingFixture API;

    public ScheduleMeetingTests(ScheduleMeetingFixture api) => API = api;

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task UpdateCommand_Should_Succeed()
    {
        await API
            .Given(
                URI($"{MeetingsManagementApi.MeetingsUrl}/{API.MeetingId}/schedule"),
                BODY(DateRange.Create(Start, End)),
                HEADERS(IF_MATCH(1))
            )
            .When(POST)
            .Then(OK);

        await API
            .Given(
                URI($"{MeetingsManagementApi.MeetingsUrl}/{API.MeetingId}")
            )
            .When(GET_UNTIL(RESPONSE_ETAG_IS(2)))
            .Then(
                OK,
                RESPONSE_BODY<MeetingView>(meeting =>
                {
                    meeting.Id.Should().Be(API.MeetingId);
                    meeting.Name.Should().Be(API.MeetingName);
                    meeting.Start.Should().Be(Start);
                    meeting.End.Should().Be(End);
                }));
    }

    private readonly DateTime Start = DateTime.UtcNow;
    private readonly DateTime End = DateTime.UtcNow;
}
