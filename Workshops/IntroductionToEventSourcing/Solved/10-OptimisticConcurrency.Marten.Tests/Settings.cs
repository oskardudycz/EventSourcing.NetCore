using Core.Testing.Fixtures;
using JasperFx.CommandLine;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestFramework(typeof(OptimisticConcurrency.Marten.Tests.AssemblyFixture))]
[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(OptimisticConcurrency.Marten.Tests.TestPostgres))]

namespace OptimisticConcurrency.Marten.Tests;

public sealed class AssemblyFixture : XunitTestFramework
{
    public AssemblyFixture() =>
        JasperFxEnvironment.AutoStartHost = true;
}

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__ShoppingCarts");
