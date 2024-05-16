using Core.Events;
using Quartz;

namespace Core.Scheduling;

public class PassageOfTimeJob(IEventBus eventBus, TimeProvider timeProvider): IJob
{
    public Task Execute(IJobExecutionContext context) =>
        eventBus.Publish(
            EventEnvelope.From(new TimeHasPassed(timeProvider.GetUtcNow(), context.PreviousFireTimeUtc)),
            CancellationToken.None
        );
}
