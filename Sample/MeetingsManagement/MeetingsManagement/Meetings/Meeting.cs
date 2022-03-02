using System;
using Core.Aggregates;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.SchedulingMeeting;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings;

public class Meeting: Aggregate
{
    public string Name { get; private set; } = default!;

    public DateTime Created { get; private set; }

    public DateRange? Occurs { get; private set; }

    public Meeting()
    {
    }

    public static Meeting New(Guid id, string name)
    {
        return new(id, name, DateTime.UtcNow);
    }

    public Meeting(Guid id, string name, DateTime created)
    {
        if (id == Guid.Empty)
            throw new ArgumentException($"{nameof(id)} cannot be empty.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"{nameof(name)} cannot be empty.");

        var @event = MeetingCreated.Create(id, name, created);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(MeetingCreated @event)
    {
        Id = @event.MeetingId;
        Name = @event.Name;
        Created = @event.Created;

        Version++;
    }

    internal void Schedule(DateRange occurs)
    {
        var @event = MeetingScheduled.Create(Id, occurs);

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(MeetingScheduled @event)
    {
        Occurs = @event.Occurs;

        Version++;
    }
}
