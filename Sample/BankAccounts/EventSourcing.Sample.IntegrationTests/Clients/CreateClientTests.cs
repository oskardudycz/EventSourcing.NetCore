using System;
using System.Net;
using System.Threading.Tasks;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.Queries;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;
using EventSourcing.Sample.IntegrationTests.Infrastructure;
using EventSourcing.Sample.Transactions.Views.Clients;
using FluentAssertions;
using Xunit;

namespace EventSourcing.Sample.IntegrationTests.Clients
{
    public class CreateClientTests
    {
        private readonly TestContext _sut;

        private const string ApiUrl = "/api/Clients";

        public CreateClientTests()
        {
            _sut = new TestContext();
        }

        [Fact]
        public async Task ClientFlowTests()
        {
            // prepare command
            var command = new CreateClient(
                null,
                new ClientInfo("test@test.pl","test"));

            // send create command
            var commandResponse = await _sut.Client.PostAsync(ApiUrl, command.ToJsonStringContent());

            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var commandResult = await commandResponse.Content.ReadAsStringAsync();
            commandResult.Should().NotBeNull();

            var createdId = commandResult.FromJson<Guid>();

            // prepare query
            var query = new GetClient(createdId);

            //send query
            var queryResponse = await _sut.Client.GetAsync(ApiUrl + $"/{createdId}/view");

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResponse.Should().NotBeNull();

            var clientView = queryResult.FromJson<ClientView>();
            clientView.Id.Should().Be(createdId);
            clientView.Name.Should().Be(command.Data.Name);
        }
    }
}
