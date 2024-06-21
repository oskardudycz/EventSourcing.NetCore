namespace Core.Events;


public abstract record TimeHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime)
{
    public record MinuteHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime): TimeHasPassed(Now, PreviousTime);
    public record HourHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime): TimeHasPassed(Now, PreviousTime);
    public record DayHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime): TimeHasPassed(Now, PreviousTime);
}
