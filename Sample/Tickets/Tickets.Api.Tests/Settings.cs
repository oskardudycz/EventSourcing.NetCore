using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(Tickets.Api.Tests.TestPostgres))]

namespace Tickets.Api.Tests;

public class TestPostgres(): PostgresContainerFixture("EventStore__ConnectionString");
