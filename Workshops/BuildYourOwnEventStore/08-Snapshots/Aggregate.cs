using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace EventStoreBasics;

public class Aggregate: IAggregate
{
    public Guid Id { get; protected set; }

    public int Version { get; protected set; }

    [JsonIgnore]
    private readonly List<object> uncommittedEvents = new List<object>();

    //for serialization purposes
    protected Aggregate() { }

    IEnumerable<object> IAggregate.DequeueUncommittedEvents()
    {
        var dequeuedEvents = uncommittedEvents.ToList();

        uncommittedEvents.Clear();

        return dequeuedEvents;
    }

    protected void Enqueue(object @event)
    {
        Version++;
        uncommittedEvents.Add(@event);
    }
}