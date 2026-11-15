using Shouldly;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.ValueObjects;
using Xunit;
using Ogooreck.API;
using static Ogooreck.API.ApiSpecification;

namespace MeetingsManagement.IntegrationTests.Meetings.SchedulingMeetings;

public class ScheduleMeetingTests(ApiSpecification<Program> api): IClassFixture<ApiSpecification<Program>>
{
    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task UpdateCommand_Should_Succeed()
    {
        await api
            .Given(
                "Created Meeting",
                SEND(POST, URI(MeetingsManagementApi.MeetingsUrl), BODY(new CreateMeeting(MeetingId, MeetingName)))
            )
            .When(
                POST,
                URI(ctx => $"{MeetingsManagementApi.MeetingsUrl}/{ctx.GetCreatedId()}/schedule"),
                BODY(DateRange.Create(Start, End)),
                HEADERS(IF_MATCH(1))
            )
            .Then(OK)
            .AndWhen(GET, URI(ctx => $"{MeetingsManagementApi.MeetingsUrl}/{ctx.GetCreatedId()}"))
            .Until(RESPONSE_ETAG_IS(2))
            .Then(
                OK,
                RESPONSE_BODY<MeetingView>(meeting =>
                {
                    meeting.Id.ShouldBe(MeetingId);
                    meeting.Name.ShouldBe(MeetingName);
                    meeting.Start.ShouldBe(Start);
                    meeting.End.ShouldBe(End);
                }));
    }

    private readonly DateTime Start = DateTime.UtcNow;
    private readonly DateTime End = DateTime.UtcNow;

    public readonly Guid MeetingId = Guid.CreateVersion7();
    public readonly string MeetingName = "Event Sourcing Workshop";
}
