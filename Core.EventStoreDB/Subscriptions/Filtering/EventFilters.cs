using System.Text.RegularExpressions;
using Core.Events;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using EventStore.Client;

namespace Core.EventStoreDB.Subscriptions.Filtering;

public static class EventFilters
{
    public static readonly Regex ExcludeSystemAndCheckpointEventsRegex =
        new(@"^(?!\$)(?!" + Regex.Escape(typeof(CheckpointStored).FullName!) + "$).+");

    public static Regex OneOfEventTypesRegex(params string[] values) =>
        new("^(" + string.Join("|", values.Select(Regex.Escape)) + ")$");

    public static Regex OneOfEventTypesRegex(EventTypeMapper eventTypeMapper, params Type[] eventTypes) =>
        OneOfEventTypesRegex(eventTypes.Select(eventTypeMapper.ToName).ToArray());

    public static Regex OneOfEventTypesRegex(params Type[] eventTypes) =>
        OneOfEventTypesRegex(EventTypeMapper.Instance, eventTypes);
    
    public static readonly IEventFilter ExcludeSystemAndCheckpointEvents =
        EventTypeFilter.RegularExpression(ExcludeSystemAndCheckpointEventsRegex);

    public static IEventFilter OneOfEventTypes(params string[] values) =>
        EventTypeFilter.RegularExpression(OneOfEventTypesRegex(values));

    public static IEventFilter OneOfEventTypes(EventTypeMapper eventTypeMapper, params Type[] eventTypes) =>
        EventTypeFilter.RegularExpression(OneOfEventTypesRegex(eventTypeMapper, eventTypes));

    public static IEventFilter OneOfEventTypes(params Type[] eventTypes) =>
        EventTypeFilter.RegularExpression(OneOfEventTypesRegex(eventTypes));
}
