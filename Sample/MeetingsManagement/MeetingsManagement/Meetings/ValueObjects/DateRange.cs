namespace MeetingsManagement.Meetings.ValueObjects;

public class DateRange(DateTime start, DateTime end)
{
    public DateTime Start { get; } = start;
    public DateTime End { get; } = end;

    public static DateRange Create(DateTime start, DateTime end)
    {
        if (start == default(DateTime))
            throw new ArgumentException($"{nameof(start)} needs to be defined.");

        if (end == default(DateTime))
            throw new ArgumentException($"{nameof(end)} needs to be defined.");

        if (start > end)
            throw new ArgumentException($"{nameof(start)} needs to be earlier or equal {nameof(end)}.");

        return new DateRange(start, end);
    }
}
