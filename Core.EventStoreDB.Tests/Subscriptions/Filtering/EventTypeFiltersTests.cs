using Core.Events;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.EventStoreDB.Subscriptions.Filtering;
using FluentAssertions;
using Xunit;

namespace Core.EventStoreDB.Tests.Subscriptions.Filtering;

using static EventFilters;

public class EventTypeFiltersTests
{
    [Fact]
    public void ExcludeSystemAndCheckpointEventsRegex_Should_NotMatch_SystemEvents()
    {
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("$scavengeIndexInitialized").Should().BeFalse();
        ExcludeSystemAndCheckpointEventsRegex.IsMatch(typeof(CheckpointStored).FullName!).Should().BeFalse();
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("Core.EventStoreDB.Subscriptions.Checkpoints.CheckpointStored")
            .Should().BeFalse();
    }

    [Fact]
    public void ExcludeSystemAndCheckpointEventsRegex_Should_Match_OtherEvents()
    {
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("ShoppingCartOpened").Should().BeTrue();
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("SomeOtherEvent").Should().BeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_Should_Match_ProvidedEvents()
    {
        var regex = OneOfEventTypesRegex("ShoppingCartOpened", "OrderPlaced");

        regex.IsMatch("ShoppingCartOpened").Should().BeTrue();
        regex.IsMatch("OrderPlaced").Should().BeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_Should_NotMatch_OtherEvents()
    {
        var regex = OneOfEventTypesRegex("ShoppingCartOpened", "OrderPlaced");

        regex.IsMatch("OrderCancelled").Should().BeFalse();
        regex.IsMatch("$systemEvent").Should().BeFalse();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_Match_ProvidedEventTypesWithCustomMap()
    {
        var eventTypeMapper = new EventTypeMapper();
        eventTypeMapper.AddCustomMap<ShoppingCartOpened>("ShoppingCartOpened");

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch("ShoppingCartOpened").Should().BeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_NotMatch_EventTypesWithDefaultName()
    {
        var eventTypeMapper = new EventTypeMapper();
        eventTypeMapper.AddCustomMap<ShoppingCartOpened>("ShoppingCartOpened");

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch(typeof(ShoppingCartOpened).FullName!).Should().BeFalse();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_NotMatch_OtherEventTypesWithCustomMap()
    {
        var eventTypeMapper = new EventTypeMapper();
        eventTypeMapper.AddCustomMap<ShoppingCartOpened>("ShoppingCartOpened");

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch("OrderPlaced").Should().BeFalse();
        regex.IsMatch(typeof(ShoppingCartOpened).FullName!).Should().BeFalse();
        regex.IsMatch(typeof(OrderPlaced).FullName!).Should().BeFalse();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_Match_ProvidedEventTypes()
    {
        var eventTypeMapper = new EventTypeMapper();

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch(typeof(ShoppingCartOpened).FullName!).Should().BeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_NotMatch_OtherEventTypes()
    {
        var eventTypeMapper = new EventTypeMapper();

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch("OrderPlaced").Should().BeFalse();
        regex.IsMatch(typeof(OrderPlaced).FullName!).Should().BeFalse();
    }

    private record ShoppingCartOpened(
        Guid ShoppingCartId,
        Guid ClientId
    );

    private record OrderPlaced(
        Guid OrderId,
        Guid ClientId
    );
}
