using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(ECommerce.Api.Tests.TestKurrentDB))]
[assembly: AssemblyFixture(typeof(ECommerce.Api.Tests.TestPostgres))]

namespace ECommerce.Api.Tests;

public class TestKurrentDB(): KurrentDBContainerFixture("KurrentDB__ConnectionString");

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__ECommerceDB");
