using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(OptimisticConcurrency.KurrentDB.Tests.TestKurrentDB))]

namespace OptimisticConcurrency.KurrentDB.Tests;

public class TestKurrentDB(): KurrentDBContainerFixture("ConnectionStrings__ShoppingCarts");
