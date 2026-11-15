using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(Carts.Api.Tests.TestPostgres))]

namespace Carts.Api.Tests;

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__carts", "EventStore__ConnectionString");
