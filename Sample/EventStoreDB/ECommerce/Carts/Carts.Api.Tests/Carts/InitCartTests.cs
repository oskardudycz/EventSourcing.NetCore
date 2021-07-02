using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Carts.Api.Requests.Carts;
using Carts.Carts;
using Carts.Carts.Events;
using Carts.Carts.Projections;
//using Carts.Carts.Projections;
using Core.Testing;
using FluentAssertions;
using Xunit;

namespace Carts.Api.Tests.Carts
{
    public class InitCartFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl => "/api/Carts";

        public readonly Guid ClientId = Guid.NewGuid();

        public HttpResponseMessage CommandResponse = default!;

        public override async Task InitializeAsync()
        {
            CommandResponse = await Post(new InitCartRequest {ClientId = ClientId });
        }
    }

    public class InitCartTests: IClassFixture<InitCartFixture>
    {
        private readonly InitCartFixture fixture;

        public InitCartTests(InitCartFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_CartId()
        {
            var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var createdId = await commandResponse.GetResultFromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldCreate_Cart()
        {
            var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();

            // prepare query
            var query = $"{createdId}";

            //send query
            var queryResponse = await fixture.Get(query, 10);
            queryResponse.EnsureSuccessStatusCode();

            var cartDetails = await queryResponse.GetResultFromJson<CartDetails>();
            cartDetails.Id.Should().Be(createdId);
            cartDetails.Status.Should().Be(CartStatus.Pending);
            cartDetails.ClientId.Should().Be(fixture.ClientId);
            cartDetails.Version.Should().Be(1);
            cartDetails.ProductItems.Should().BeEmpty();
            cartDetails.TotalPrice.Should().Be(0);
        }
    }
}
