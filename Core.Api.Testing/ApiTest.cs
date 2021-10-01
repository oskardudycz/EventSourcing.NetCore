using Xunit;
using Xunit.Abstractions;

namespace Core.Api.Testing
{
    public abstract class ApiTest<TFixture> : IClassFixture<TFixture>
        where TFixture : ApiFixture
    {
        protected readonly TFixture Fixture;

        protected ApiTest(TFixture fixture, ITestOutputHelper testOutputHelper)
        {
            Fixture = fixture;
            Fixture.CreateTestContext(testOutputHelper);
        }
    }
}
