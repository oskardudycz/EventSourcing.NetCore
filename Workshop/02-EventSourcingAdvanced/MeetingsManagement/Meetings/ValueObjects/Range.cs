using System;

namespace MeetingsManagement.Meetings.ValueObjects
{
    public class Range
    {
        public DateTime Start { get; }
        public DateTime End { get; }

        public Range(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public static Range Create(DateTime start, DateTime end)
        {
            if (start == default(DateTime))
                throw new ArgumentException($"{nameof(start)} needs to be defined.");

            if (end == default(DateTime))
                throw new ArgumentException($"{nameof(end)} needs to be defined.");

            if (start > end)
                throw new ArgumentException($"{nameof(start)} needs to be earlier or equal {nameof(end)}.");

            return new Range(start, end);
        }
    }
}
