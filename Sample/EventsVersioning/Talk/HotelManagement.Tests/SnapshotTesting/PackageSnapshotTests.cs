using HotelManagement.GuestStayAccounts;
using PublicApiGenerator;

namespace EventsVersioning.Tests.SnapshotTesting;

public class PackageSnapshotTests
{
    [Fact(Skip = "not now, my friend")]
    public Task my_assembly_has_no_public_api_changes()
    {
        var publicApi = typeof(GuestStayAccountEvent).Assembly.GeneratePublicApi();

        return Verify(publicApi);
    }
}
