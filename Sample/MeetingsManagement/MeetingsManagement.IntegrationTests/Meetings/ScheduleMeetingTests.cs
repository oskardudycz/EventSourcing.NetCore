using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Meetings.IntegrationTests.MeetingsManagement;
using MeetingsManagement.Api;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;
using MeetingsManagement.Meetings.Views;
using Xunit;

namespace MeetingsManagement.IntegrationTests.Meetings
{
    public class ScheduleMeetingFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl => MeetingsManagementApi.MeetingsUrl;

        public readonly Guid MeetingId = Guid.NewGuid();
        public readonly string MeetingName = "Event Sourcing Workshop";
        public readonly DateTime Start = DateTime.UtcNow;
        public readonly DateTime End = DateTime.UtcNow;

        public HttpResponseMessage CommandResponse = default!;

        public override async Task InitializeAsync()
        {
            // prepare command
            var createCommand = new CreateMeeting(
                MeetingId,
                MeetingName
            );

            // send create command
            await PostAsync( createCommand);

            var occurs = DateRange.Create(Start, End);

            // send schedule meeting request
            CommandResponse = await PostAsync($"{MeetingId}/schedule", occurs);
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
        [Trait("Category", "Exercise")]
        public async Task ScheduleMeeting_ShouldReturn_CreatedStatus_With_MeetingId()
        {
            var commandResponse = fixture.CommandResponse;
            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // get created record id
            var commandResult = await commandResponse.Content.ReadAsStringAsync();
            commandResult.Should().BeEmpty();
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task ScheduleMeeting_ShouldUpdateReadModel()
        {
            // prepare query
            var query = new GetMeeting(fixture.MeetingId);

            //send query
            var queryResponse = await fixture.GetAsync($"{fixture.MeetingId}");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResult.Should().NotBeNull();

            var meeting = queryResult.FromJson<MeetingView>();
            meeting.Id.Should().Be(fixture.MeetingId);
            meeting.Name.Should().Be(fixture.MeetingName);
            meeting.Start.Should().Be(fixture.Start);
            meeting.End.Should().Be(fixture.End);
        }
    }
}
