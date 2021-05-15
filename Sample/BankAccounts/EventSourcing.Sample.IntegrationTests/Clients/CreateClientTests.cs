using System;
using System.Net;
using System.Threading.Tasks;
using EventSourcing.Sample.Clients.Contracts.Clients.Commands;
using EventSourcing.Sample.Clients.Contracts.Clients.ValueObjects;
using EventSourcing.Sample.Transactions.Views.Clients;
using FluentAssertions;
using Xunit;
using Core.Testing;
using EventSourcing.Web.Sample;

namespace EventSourcing.Sample.IntegrationTests.Clients
{
    public class CreateClientTests
    {
        private readonly TestContext<Startup> sut;

        private const string ApiUrl = "/api/Clients";

        public CreateClientTests()
        {
            sut = new TestContext<Startup>();
        }

        [Fact]
        public async Task ClientFlowTests()
        {
            // prepare command
            var command = new CreateClient(
                Guid.NewGuid(),
                new ClientInfo("test@test.pl","test"));

            // send create command
            var commandResponse = await sut.Client.PostAsync(ApiUrl, command.ToJsonStringContent());

            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var createdId = await commandResponse.GetResultFromJson<Guid>();

            //send query
            var queryResponse = await sut.Client.GetAsync(ApiUrl + $"/{createdId}/view");

            var clientView = await queryResponse.GetResultFromJson<ClientView>();
            clientView.Id.Should().Be(createdId);
            clientView.Name.Should().Be(command.Data.Name);
        }
    }
}
