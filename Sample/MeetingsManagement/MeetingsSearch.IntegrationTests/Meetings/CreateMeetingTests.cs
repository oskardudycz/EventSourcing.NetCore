using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using MeetingsSearch.Meetings;
using MeetingsSearch.Meetings.Events;
using Xunit;

namespace MeetingsSearch.IntegrationTests.Meetings
{
    public class CreateMeetingFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl { get; } = MeetingsSearchApi.MeetingsUrl;

        public readonly Guid MeetingId = Guid.NewGuid();
        public readonly string MeetingName = "Event Sourcing Workshop";

        public override async Task InitializeAsync()
        {
            // prepare event
            var @event = new MeetingCreated(
                MeetingId,
                MeetingName
            );

            // send meeting created event to internal event bus
            await Sut.PublishInternalEvent(@event);
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
        [Trait("Category", "Exercise")]
        public async Task MeetingCreated_ShouldUpdateReadModel()
        {
            //send query
            var queryResponse = await fixture.Get($"{MeetingsSearchApi.MeetingsUrl}");
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResult.Should().NotBeNull();

            await Task.Delay(5000);

            var meetings = queryResult.FromJson<IReadOnlyCollection<Meeting>>();
            meetings.Should().Contain(meeting =>
                meeting.Id == fixture.MeetingId
                && meeting.Name == fixture.MeetingName
            );
        }
    }
}
