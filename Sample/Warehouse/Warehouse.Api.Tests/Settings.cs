using Core.Testing.Fixtures;
using Xunit;

[assembly: AssemblyFixture(typeof(Warehouse.Api.Tests.TestPostgres))]

namespace Warehouse.Api.Tests;

public class TestPostgres(): PostgresContainerFixture("ConnectionStrings__WarehouseDB");
