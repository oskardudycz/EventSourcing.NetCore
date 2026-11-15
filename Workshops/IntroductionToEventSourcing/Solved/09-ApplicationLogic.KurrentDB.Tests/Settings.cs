using Core.Testing.Fixtures;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

[assembly: Parallelization(Mode = ParallelMode.None)]
[assembly: AssemblyFixture(typeof(ApplicationLogic.KurrentDB.Tests.TestKurrentDB))]

namespace ApplicationLogic.KurrentDB.Tests;

public class TestKurrentDB(): KurrentDBContainerFixture("ConnectionStrings__ShoppingCarts");
