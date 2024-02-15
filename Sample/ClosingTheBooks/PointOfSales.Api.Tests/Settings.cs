using Oakton;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: TestFramework("PointOfSales.Api.Tests.AssemblyFixture", "PointOfSales.Api.Tests")]

namespace PointOfSales.Api.Tests;

public sealed class AssemblyFixture : XunitTestFramework
{
    public AssemblyFixture(IMessageSink messageSink)
        :base(messageSink)
    {
        OaktonEnvironment.AutoStartHost = true;
    }
}
