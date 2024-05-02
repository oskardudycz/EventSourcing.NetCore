using Oakton;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("ApplicationLogic.EventStoreDB.Tests.AssemblyFixture", "ApplicationLogic.EventStoreDB.Tests")]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ApplicationLogic.EventStoreDB.Tests;

public sealed class AssemblyFixture : XunitTestFramework
{
    public AssemblyFixture(IMessageSink messageSink)
        :base(messageSink)
    {
        OaktonEnvironment.AutoStartHost = true;
    }
}
