using Oakton;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: TestFramework("OptimisticConcurrency.Marten.Tests.AssemblyFixture", "OptimisticConcurrency.Marten.Tests")]

namespace OptimisticConcurrency.Marten.Tests;

public sealed class AssemblyFixture : XunitTestFramework
{
    public AssemblyFixture(IMessageSink messageSink)
        :base(messageSink)
    {
        OaktonEnvironment.AutoStartHost = true;
    }
}
