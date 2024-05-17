using ECommerce.V1;
using PublicApiGenerator;

namespace EventsVersioning.Tests.SnapshotTesting;

public class PackageSnapshotTests
{
    [Fact]
    public Task my_assembly_has_no_public_api_changes()
    {
        var publicApi = typeof(ShoppingCartOpened).Assembly.GeneratePublicApi();

        return Verify(publicApi);
    }
}
