using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(Carts.Api.Tests.TestKurrentDB))]
[assembly: AssemblyFixture(typeof(Carts.Api.Tests.TestPostgres))]

namespace Carts.Api.Tests;

public class TestKurrentDB(): KurrentDBContainerFixture("KurrentDB__ConnectionString");

public class TestPostgres(): PostgresContainerFixture("ReadModel_Marten__ConnectionString");
