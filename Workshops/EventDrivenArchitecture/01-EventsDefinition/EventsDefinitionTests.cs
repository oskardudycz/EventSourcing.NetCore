using FluentAssertions;
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
        const int minimumExpectedEventTypesCount = 5;
        events.Should().HaveCountGreaterThan(minimumExpectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCount(minimumExpectedEventTypesCount);
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
        const int minimumExpectedEventTypesCount = 3;
        events.Should().HaveCountGreaterThan(minimumExpectedEventTypesCount);
        events.GroupBy(e => e.GetType()).Should().HaveCount(minimumExpectedEventTypesCount);
    }
}
