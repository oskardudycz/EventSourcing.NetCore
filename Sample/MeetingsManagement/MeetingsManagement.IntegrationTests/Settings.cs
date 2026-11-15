using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(MeetingsManagement.IntegrationTests.TestPostgres))]

namespace MeetingsManagement.IntegrationTests;

public class TestPostgres(): PostgresContainerFixture("EventStore__ConnectionString");
