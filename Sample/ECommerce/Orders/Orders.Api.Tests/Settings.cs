using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(Orders.Api.Tests.TestPostgres))]

namespace Orders.Api.Tests;

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__orders", "EventStore__ConnectionString");
