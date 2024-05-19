using Oakton;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("ApplicationLogic.Marten.Tests.AssemblyFixture", "ApplicationLogic.Marten.Tests")]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ApplicationLogic.Marten.Tests;

public sealed class AssemblyFixture : XunitTestFramework
{
    public AssemblyFixture(IMessageSink messageSink)
        :base(messageSink) =>
        OaktonEnvironment.AutoStartHost = true;
}
