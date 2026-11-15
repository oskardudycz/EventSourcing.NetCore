using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(Payments.Api.Tests.TestPostgres))]

namespace Payments.Api.Tests;

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__payments", "EventStore__ConnectionString");
