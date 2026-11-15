using Core.Events;
using Core.KurrentDB.Subscriptions.Checkpoints;
using Core.KurrentDB.Subscriptions.Filtering;
using Shouldly;
using Xunit;

namespace Core.KurrentDB.Tests.Subscriptions.Filtering;

using static EventFilters;

public class EventTypeFiltersTests
{
    [Fact]
    public void ExcludeSystemAndCheckpointEventsRegex_Should_NotMatch_SystemEvents()
    {
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("$scavengeIndexInitialized").ShouldBeFalse();
        ExcludeSystemAndCheckpointEventsRegex.IsMatch(typeof(CheckpointStored).FullName!).ShouldBeFalse();
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("Core.KurrentDB.Subscriptions.Checkpoints.CheckpointStored")
            .ShouldBeFalse();
    }

    [Fact]
    public void ExcludeSystemAndCheckpointEventsRegex_Should_Match_OtherEvents()
    {
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("ShoppingCartOpened").ShouldBeTrue();
        ExcludeSystemAndCheckpointEventsRegex.IsMatch("SomeOtherEvent").ShouldBeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_Should_Match_ProvidedEvents()
    {
        var regex = OneOfEventTypesRegex("ShoppingCartOpened", "OrderPlaced");

        regex.IsMatch("ShoppingCartOpened").ShouldBeTrue();
        regex.IsMatch("OrderPlaced").ShouldBeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_Should_NotMatch_OtherEvents()
    {
        var regex = OneOfEventTypesRegex("ShoppingCartOpened", "OrderPlaced");

        regex.IsMatch("OrderCancelled").ShouldBeFalse();
        regex.IsMatch("$systemEvent").ShouldBeFalse();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_Match_ProvidedEventTypesWithCustomMap()
    {
        var eventTypeMapper = new EventTypeMapper();
        eventTypeMapper.AddCustomMap<ShoppingCartOpened>("ShoppingCartOpened");

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch("ShoppingCartOpened").ShouldBeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_NotMatch_EventTypesWithDefaultName()
    {
        var eventTypeMapper = new EventTypeMapper();
        eventTypeMapper.AddCustomMap<ShoppingCartOpened>("ShoppingCartOpened");

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch(typeof(ShoppingCartOpened).FullName!).ShouldBeFalse();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_NotMatch_OtherEventTypesWithCustomMap()
    {
        var eventTypeMapper = new EventTypeMapper();
        eventTypeMapper.AddCustomMap<ShoppingCartOpened>("ShoppingCartOpened");

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch("OrderPlaced").ShouldBeFalse();
        regex.IsMatch(typeof(ShoppingCartOpened).FullName!).ShouldBeFalse();
        regex.IsMatch(typeof(OrderPlaced).FullName!).ShouldBeFalse();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_Match_ProvidedEventTypes()
    {
        var eventTypeMapper = new EventTypeMapper();

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch(typeof(ShoppingCartOpened).FullName!).ShouldBeTrue();
    }

    [Fact]
    public void OneOfEventTypesRegex_WithEventTypeMapper_Should_NotMatch_OtherEventTypes()
    {
        var eventTypeMapper = new EventTypeMapper();

        var regex = OneOfEventTypesRegex(eventTypeMapper, typeof(ShoppingCartOpened));

        regex.IsMatch("OrderPlaced").ShouldBeFalse();
        regex.IsMatch(typeof(OrderPlaced).FullName!).ShouldBeFalse();
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
