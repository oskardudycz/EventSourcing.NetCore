using Oakton;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: TestFramework("Helpdesk.Api.Tests.AssemblyFixture", "Helpdesk.Api.Tests")]

namespace Helpdesk.Api.Tests;

public sealed class AssemblyFixture : XunitTestFramework, IDisposable
{
    public AssemblyFixture(IMessageSink messageSink)
        :base(messageSink)
    {
        OaktonEnvironment.AutoStartHost = true;
    }

    public new void Dispose()
    {
        base.Dispose();
    }
}
