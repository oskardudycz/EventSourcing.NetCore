using System;
using System.Net;
using System.Threading.Tasks;
using EventSourcing.Sample.IntegrationTests.Infrastructure;
using FluentAssertions;
using Meetings.IntegrationTests.Infrastructure;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;
using Xunit;

namespace EventSourcing.Sample.IntegrationTests.Meetings
{
    public class CreateMeetingTests
    {
        private readonly TestContext _sut;

        private const string ApiUrl = "/api/Meetings";

        public CreateMeetingTests()
        {
            _sut = new TestContext();
        }

        [Fact]
        public async Task IssueFlowTests()
        {
            // prepare command
            var command = new CreateMeeting(
                Guid.NewGuid(),
                "Event Sourcing Workshop");

            // send create command
            var commandResponse = await _sut.Client.PostAsync(ApiUrl, command.ToJsonStringContent());

            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var commandResult = await commandResponse.Content.ReadAsStringAsync();
            commandResult.Should().NotBeNull();

            var createdId = commandResult.FromJson<Guid>();

            // prepare query
            var query = new GetMeeting(createdId);

            //send query
            var queryResponse = await _sut.Client.GetAsync(ApiUrl + $"/{createdId}/view");

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResponse.Should().NotBeNull();

            var MeetingView = queryResult.FromJson<MeetingSummary>();
            MeetingView.Id.Should().Be(createdId);
            MeetingView.Name.Should().Be(command.Name);
        }
    }
}
