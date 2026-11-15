using Core.Testing.Fixtures;
using JasperFx.CommandLine;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]

[assembly: TestFramework(typeof(Helpdesk.Api.Tests.AssemblyFixture))]
[assembly: AssemblyFixture(typeof(Helpdesk.Api.Tests.TestPostgres))]

namespace Helpdesk.Api.Tests;

public sealed class AssemblyFixture : XunitTestFramework
{
    public AssemblyFixture()
    {
        JasperFxEnvironment.AutoStartHost = true;
    }
}

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__Incidents");
