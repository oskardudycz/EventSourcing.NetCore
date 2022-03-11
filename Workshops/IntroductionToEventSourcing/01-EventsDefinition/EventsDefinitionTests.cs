using FluentAssertions;
using Xunit;

namespace IntroductionToEventSourcing.EventsDefinition;

// 1. Define your events and entity here

public class EventsDefinitionTests
{
    [Fact]
    [Trait("Category", "SkipCI")]
    public void AllEventTypes_ShouldBeDefined()
    {
        var events = new object[]
        {
            // 2. Put your sample events here
        };

        const int expectedEventTypesCount = 5;
        events.Should().HaveCount(expectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCount(expectedEventTypesCount);
    }
}
