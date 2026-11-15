using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(Shipments.Api.Tests.TestPostgres))]

namespace Shipments.Api.Tests;

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__ShipmentsDatabase");
