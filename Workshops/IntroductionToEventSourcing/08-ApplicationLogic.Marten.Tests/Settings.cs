using Oakton;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: TestFramework("ApplicationLogic.Marten.Tests.AssemblyFixture", "ApplicationLogic.Marten.Tests")]

namespace ApplicationLogic.Marten.Tests;

public sealed class AssemblyFixture : XunitTestFramework
{
    public AssemblyFixture(IMessageSink messageSink)
        :base(messageSink)
    {
        OaktonEnvironment.AutoStartHost = true;
    }
}
