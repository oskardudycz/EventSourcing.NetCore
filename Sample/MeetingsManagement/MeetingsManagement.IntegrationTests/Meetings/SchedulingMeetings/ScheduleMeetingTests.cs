using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Api.Testing;
using FluentAssertions;
using MeetingsManagement.Api;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.ValueObjects;
using Xunit;

namespace MeetingsManagement.IntegrationTests.Meetings.SchedulingMeetings;

public class ScheduleMeetingFixture: ApiFixture<Startup>
{
    protected override string ApiUrl => MeetingsManagementApi.MeetingsUrl;

    public readonly Guid MeetingId = Guid.NewGuid();
    public readonly string MeetingName = "Event Sourcing Workshop";
    public readonly DateTime Start = DateTime.UtcNow;
    public readonly DateTime End = DateTime.UtcNow;

    public HttpResponseMessage CreateMeetingCommandResponse = default!;
    public HttpResponseMessage ScheduleMeetingCommandResponse = default!;

    public override async Task InitializeAsync()
    {
        // prepare command
        var createCommand = new CreateMeeting(
            MeetingId,
            MeetingName
        );

        // send create command
        CreateMeetingCommandResponse = await Post(createCommand);

        var occurs = DateRange.Create(Start, End);

        // send schedule meeting request
        ScheduleMeetingCommandResponse =
            await Post($"{MeetingId}/schedule", occurs, new RequestOptions { IfMatch = 1.ToString() });
    }
}

public class ScheduleMeetingTests: IClassFixture<ScheduleMeetingFixture>
{
    private readonly ScheduleMeetingFixture fixture;

    public ScheduleMeetingTests(ScheduleMeetingFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task CreateMeeting_ShouldReturn_CreatedStatus_With_MeetingId()
    {
        var commandResponse = fixture.CreateMeetingCommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdId = await commandResponse.GetResultFromJson<Guid>();
        createdId.Should().NotBeEmpty();
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public void ScheduleMeeting_ShouldSucceed()
    {
        var commandResponse = fixture.ScheduleMeetingCommandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task ScheduleMeeting_ShouldUpdateReadModel()
    {
        //send query
        var queryResponse = await fixture.Get($"{fixture.MeetingId}");
        queryResponse.EnsureSuccessStatusCode();

        var meeting = await queryResponse.GetResultFromJson<MeetingView>();
        meeting.Id.Should().Be(fixture.MeetingId);
        meeting.Name.Should().Be(fixture.MeetingName);
        meeting.Start.Should().Be(fixture.Start);
        meeting.End.Should().Be(fixture.End);
    }
}
