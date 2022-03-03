using System.Net;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using MeetingsManagement.Api;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using Xunit;

namespace MeetingsManagement.IntegrationTests.Meetings.CreatingMeeting;

public class CreateMeetingFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => MeetingsManagementApi.MeetingsUrl;

    public readonly Guid MeetingId = Guid.NewGuid();
    public readonly string MeetingName = "Event Sourcing Workshop";

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        // prepare command
        var command = new CreateMeeting(
            MeetingId,
            MeetingName
        );

        // send create command
        CommandResponse = await Post(command);
    }
}

public class CreateMeetingTests: IClassFixture<CreateMeetingFixture>
{
    private readonly CreateMeetingFixture fixture;

    public CreateMeetingTests(CreateMeetingFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task CreateCommand_ShouldReturn_CreatedStatus_With_MeetingId()
    {
        var commandResponse = fixture.CommandResponse;
        commandResponse.EnsureSuccessStatusCode();
        commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // get created record id
        var createdId = await commandResponse.GetResultFromJson<Guid>();
        createdId.Should().Be(fixture.MeetingId);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public void CreateCommand_ShouldPublish_MeetingCreateEvent()
    {
        // assert MeetingCreated event was produced to external bus
        fixture.ShouldPublishInternalEventOfType<MeetingCreated>(
            @event =>
                @event.MeetingId == fixture.MeetingId
                && @event.Name == fixture.MeetingName
            );
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task CreateCommand_ShouldUpdateReadModel()
    {
        // prepare query
        var query = $"{fixture.MeetingId}";

        //send query
        var queryResponse = await fixture.Get(query);
        queryResponse.EnsureSuccessStatusCode();

        var meetingSummary =  await queryResponse.GetResultFromJson<MeetingView>();
        meetingSummary.Id.Should().Be(fixture.MeetingId);
        meetingSummary.Name.Should().Be(fixture.MeetingName);
    }
}
