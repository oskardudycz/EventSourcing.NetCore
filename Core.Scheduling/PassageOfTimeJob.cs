using Core.Events;
using Quartz;

namespace Core.Scheduling;

using static TimeHasPassed;

public class PassageOfTimeJob(IEventBus eventBus, TimeProvider timeProvider): IJob
{
    public ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var timeUnit = context.MergedJobDataMap.GetString("timeUnit")!.ToTimeUnit();
        var @event = timeUnit.ToEvent(timeProvider.GetUtcNow(), context.PreviousFireTimeUtc);

        return new ValueTask(eventBus.Publish(EventEnvelope.From(@event), cancellationToken));
    }
}

public enum TimeUnit
{
    Minute,
    Hour,
    Day
}

public static class TimeUnitExtensions
{
    public static TimeUnit ToTimeUnit(this string timeUnitString) =>
        Enum.Parse<TimeUnit>(timeUnitString);

    public static TimeSpan ToTimeSpan(this TimeUnit timeUnit) =>
        timeUnit switch
        {
            TimeUnit.Minute => TimeSpan.FromMinutes(1),
            TimeUnit.Hour => TimeSpan.FromHours(1),
            TimeUnit.Day => TimeSpan.FromDays(1),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), $"Not expected time unit value: {timeUnit}")
        };

    public static TimeHasPassed ToEvent(this TimeUnit timeUnit, DateTimeOffset now, DateTimeOffset? previous) =>
        timeUnit switch
        {
            TimeUnit.Minute => new MinuteHasPassed(now, previous),
            TimeUnit.Hour => new HourHasPassed(now, previous),
            TimeUnit.Day => new DayHasPassed(now, previous),
            _ => throw new ArgumentOutOfRangeException(nameof(timeUnit), $"Not expected time unit value: {timeUnit}")
        };
}
