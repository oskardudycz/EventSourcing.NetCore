namespace Core.Events;


public record TimeHasPassed(DateTimeOffset Now, DateTimeOffset? PreviousTime);
