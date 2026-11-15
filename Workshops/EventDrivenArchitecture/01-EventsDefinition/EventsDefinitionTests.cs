using Shouldly;
using Xunit;

namespace EventsDefinition;

public class EventsDefinitionTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void GuestStayAccountEventTypes_AreDefined()
    {
        // Given

        // When
        var events = new object[]
        {
            // 2. TODO: Put your sample events here
        };

        // Then
        const int expectedEventTypesCount = 5;
        events.Count().ShouldBe(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Count().ShouldBe(expectedEventTypesCount);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void GroupCheckoutEventTypes_AreDefined()
    {
        // Given

        // When
        var events = new object[]
        {
            // 2. TODO: Put your sample events here
        };

        // Then
        const int expectedEventTypesCount = 5;
        events.Count().ShouldBe(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Count().ShouldBe(expectedEventTypesCount);
    }
}
