using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Api.Testing;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Warehouse.Products.GettingProducts;
using Warehouse.Products.RegisteringProduct;
using Xunit;
using Xunit.Abstractions;

namespace Warehouse.Api.Tests.Products.GettingProducts
{
    public class GetProductsFixture: ApiFixture
    {
        protected override string ApiUrl => "/api/products";

        protected override Func<IWebHostBuilder, IWebHostBuilder> SetupWebHostBuilder =>
            whb => WarehouseTestWebHostBuilder.Configure(whb, nameof(GetProductsFixture));

        public IList<ProductListItem> RegisteredProducts = new List<ProductListItem>();

        protected override async Task Setup()
        {
            var productsToRegister = new[]
            {
                new RegisterProductRequest("ZX1234", "ValidName", "ValidDescription"),
                new RegisterProductRequest("AD5678", "OtherValidName", "OtherValidDescription"),
                new RegisterProductRequest("BH90210", "AnotherValid", "AnotherValidDescription")
            };

            foreach (var registerProduct in productsToRegister)
            {
                var registerResponse = await Post(registerProduct);
                registerResponse.EnsureSuccessStatusCode()
                    .StatusCode.Should().Be(HttpStatusCode.Created);

                var createdId = await registerResponse.GetResultFromJson<Guid>();

                var (sku, name, _) = registerProduct;
                RegisteredProducts.Add(new ProductListItem(createdId, sku!, name!));
            }
        }
    }

    public class GetProductsTests: ApiTest<GetProductsFixture>
    {
        public GetProductsTests(GetProductsFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        public async Task ValidRequest_With_NoParams_ShouldReturn_200()
        {
            // Given

            // When
            var response = await Fixture.Get();

            // Then
            response.EnsureSuccessStatusCode()
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var products = await response.GetResultFromJson<IReadOnlyList<ProductListItem>>();
            products.Should().NotBeEmpty();
            products.Should().BeEquivalentTo(Fixture.RegisteredProducts);
        }

        [Fact]
        public async Task ValidRequest_With_Filter_ShouldReturn_SubsetOfRecords()
        {
            // Given
            var filteredRecord = Fixture.RegisteredProducts.First();
            var filter = Fixture.RegisteredProducts.First().Sku.Substring(1);

            // When
            var response = await Fixture.Get($"?filter={filter}");

            // Then
            response.EnsureSuccessStatusCode()
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var products = await response.GetResultFromJson<IReadOnlyList<ProductListItem>>();
            products.Should().NotBeEmpty();
            products.Should().BeEquivalentTo(new List<ProductListItem>{filteredRecord});
        }



        [Fact]
        public async Task ValidRequest_With_Paging_ShouldReturn_PageOfRecords()
        {
            // Given
            const int page = 2;
            const int pageSize = 1;
            var filteredRecords = Fixture.RegisteredProducts
                .Skip(page - 1)
                .Take(pageSize)
                .ToList();

            // When
            var response = await Fixture.Get($"?page={page}&pageSize={pageSize}");

            // Then
            response.EnsureSuccessStatusCode()
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var products = await response.GetResultFromJson<IReadOnlyList<ProductListItem>>();
            products.Should().NotBeEmpty();
            products.Should().BeEquivalentTo(filteredRecords);
        }

        [Fact]
        public async Task NegativePage_ShouldReturn_400()
        {
            // Given
            var pageSize = -20;

            // When
            var response = await Fixture.Get($"?page={pageSize}");

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-20)]
        public async Task NegativeOrZeroPageSize_ShouldReturn_400(int pageSize)
        {
            // Given

            // When
            var response = await Fixture.Get($"?page={pageSize}");

            // Then
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
