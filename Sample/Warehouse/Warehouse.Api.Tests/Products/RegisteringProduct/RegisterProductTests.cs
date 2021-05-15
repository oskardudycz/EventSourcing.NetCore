using System;
using System.Net;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Warehouse.Products.RegisteringProduct;
using Xunit;

namespace Warehouse.Api.Tests.Products.RegisteringProduct
{
    public class RegisteringProductTests
    {
        public class RegisterProductFixture: ApiFixture
        {
            protected override string ApiUrl => "/api/products";

            protected override Func<IWebHostBuilder, IWebHostBuilder> SetupWebHostBuilder =>
                whb => WarehouseTestWebHostBuilder.Configure(whb, nameof(RegisterProductFixture));
        }

        public class RegisterProductTests: IClassFixture<RegisterProductFixture>
        {
            private readonly RegisterProductFixture fixture;

            public RegisterProductTests(RegisterProductFixture fixture)
            {
                this.fixture = fixture;
            }

            [Theory]
            [MemberData(nameof(ValidRequests))]
            public async Task ValidRequest_ShouldReturn_201(RegisterProductRequest validRequest)
            {
                // Given

                // When
                var response = await fixture.Post(validRequest);

                // Then
                response.EnsureSuccessStatusCode()
                    .StatusCode.Should().Be(HttpStatusCode.Created);

                var createdId = await response.GetResultFromJson<Guid>();
                createdId.Should().NotBeEmpty();
            }

            [Theory]
            [MemberData(nameof(InvalidRequests))]
            public async Task InvalidRequest_ShouldReturn_400(RegisterProductRequest invalidRequest)
            {
                // Given

                // When
                var response = await fixture.Post(invalidRequest);

                // Then
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            }

            [Fact]
            public async Task RequestForExistingSKUShouldFail_ShouldReturn_409()
            {
                // Given
                var request = new RegisterProductRequest("AA2039485", ValidName, ValidDescription);

                var response = await fixture.Post(request);
                response.StatusCode.Should().Be(HttpStatusCode.Created);

                // When
                response = await fixture.Post(request);

                // Then
                response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            }

            private const string ValidName = "VALID_NAME";
            private static string ValidSKU => $"CC{DateTime.Now.Ticks}";
            private const string ValidDescription = "VALID_DESCRIPTION";

            public static TheoryData<RegisterProductRequest> ValidRequests = new()
            {
                new RegisterProductRequest(ValidSKU, ValidName, ValidDescription),
                new RegisterProductRequest(ValidSKU, ValidName, null)
            };

            public static TheoryData<RegisterProductRequest> InvalidRequests = new()
            {
                new RegisterProductRequest(null, ValidName, ValidDescription),
                new RegisterProductRequest("INVALID_SKU", ValidName, ValidDescription),
                new RegisterProductRequest(ValidSKU, null, ValidDescription),
            };
        }
    }
}
