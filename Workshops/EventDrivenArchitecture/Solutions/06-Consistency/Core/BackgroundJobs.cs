namespace Consistency.Core;

public class BackgroundJobs
{
    private readonly List<Func<CancellationToken, ValueTask>> jobs = [];

    public void Register(Func<CancellationToken, ValueTask> job) =>
        jobs.Add(job);

    public async ValueTask Run(CancellationToken ct = default)
    {
        foreach (var job in jobs)
            await job(ct);
    }

    // Applications run background jobs all the time. Tests run them until they succeed and stop publishing new messages
    public async ValueTask RunUntilIdle(MessageCatcher publishedMessages, CancellationToken ct = default)
    {
        for (var run = 0; run < 100; run++)
        {
            var publishedBefore = publishedMessages.Published.Count;

            try
            {
                await Run(ct);
            }
            catch (TimeoutException)
            {
                continue;
            }

            if (publishedMessages.Published.Count == publishedBefore)
                return;
        }

        throw new InvalidOperationException("Background jobs keep failing or publishing messages");
    }
}
