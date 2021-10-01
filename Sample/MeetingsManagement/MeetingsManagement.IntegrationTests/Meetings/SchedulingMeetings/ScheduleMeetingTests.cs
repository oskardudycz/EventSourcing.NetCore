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
using Xunit.Abstractions;

namespace MeetingsManagement.IntegrationTests.Meetings.SchedulingMeetings
{
    public class ScheduleMeetingFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl => MeetingsManagementApi.MeetingsUrl;

        public readonly Guid MeetingId = Guid.NewGuid();
        public readonly string MeetingName = "Event Sourcing Workshop";
        public readonly DateTime Start = DateTime.UtcNow;
        public readonly DateTime End = DateTime.UtcNow;

        public HttpResponseMessage CreateMeetingCommandResponse = default!;
        public HttpResponseMessage ScheduleMeetingCommandResponse = default!;

        protected override async Task Setup()
        {
            // prepare command
            var createCommand = new CreateMeeting(
                MeetingId,
                MeetingName
            );

            // send create command
            CreateMeetingCommandResponse = await Post( createCommand);

            var occurs = DateRange.Create(Start, End);

            // send schedule meeting request
            ScheduleMeetingCommandResponse = await Post($"{MeetingId}/schedule", occurs);
        }
    }

    public class ScheduleMeetingTests: ApiTest<ScheduleMeetingFixture>
    {
        public ScheduleMeetingTests(ScheduleMeetingFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateMeeting_ShouldReturn_CreatedStatus_With_MeetingId()
        {
            var commandResponse = Fixture.CreateMeetingCommandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            var createdId = await commandResponse.GetResultFromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task ScheduleMeeting_ShouldSucceed()
        {
            var commandResponse = Fixture.ScheduleMeetingCommandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var createdId = await commandResponse.GetResultFromJson<string>();
            createdId.Should().BeNull();
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task ScheduleMeeting_ShouldUpdateReadModel()
        {
            //send query
            var queryResponse = await Fixture.Get($"{Fixture.MeetingId}");
            queryResponse.EnsureSuccessStatusCode();

            var meeting = await queryResponse.GetResultFromJson<MeetingView>();
            meeting.Id.Should().Be(Fixture.MeetingId);
            meeting.Name.Should().Be(Fixture.MeetingName);
            meeting.Start.Should().Be(Fixture.Start);
            meeting.End.Should().Be(Fixture.End);
        }
    }
}
