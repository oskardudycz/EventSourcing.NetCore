using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using ECommerce.Api.Requests;
using FluentAssertions;
using Xunit;

namespace ECommerce.Api.Tests.ShoppingCarts.Initializing
{
    public class InitializeShoppingCartFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl => "/api/ShoppingCarts";

        public readonly Guid ClientId = Guid.NewGuid();

        public HttpResponseMessage CommandResponse = default!;

        public override async Task InitializeAsync()
        {
            CommandResponse = await Post(new InitializeShoppingCartRequest(ClientId));
        }
    }

    public class InitializeShoppingCartTests: IClassFixture<InitializeShoppingCartFixture>
    {
        private readonly InitializeShoppingCartFixture fixture;

        public InitializeShoppingCartTests(InitializeShoppingCartFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_CartId()
        {
            var commandResponse = fixture.CommandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var createdId = await commandResponse.GetResultFromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        // [Fact]
        // [Trait("Category", "Acceptance")]
        // public async Task CreateCommand_ShouldCreate_Cart()
        // {
        //     var createdId = await fixture.CommandResponse.GetResultFromJson<Guid>();
        //
        //     // prepare query
        //     var query = $"{createdId}";
        //
        //     //send query
        //     var queryResponse = await fixture.Get(query, 10,
        //         check: response => new(response.StatusCode == HttpStatusCode.Created));
        //
        //     queryResponse.EnsureSuccessStatusCode();
        //
        //     var cartDetails = await queryResponse.GetResultFromJson<CartDetails>();
        //     cartDetails.Id.Should().Be(createdId);
        //     cartDetails.Status.Should().Be(CartStatus.Pending);
        //     cartDetails.ClientId.Should().Be(fixture.ClientId);
        //     cartDetails.Version.Should().Be(1);
        //     cartDetails.ProductItems.Should().BeEmpty();
        //     cartDetails.TotalPrice.Should().Be(0);
        // }
    }
}
