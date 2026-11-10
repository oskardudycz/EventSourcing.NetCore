namespace Consistency.Core;

// Fails at a random call, then again after 2 to 4 calls, so failures land in a different place on every run, but never twice in a row
public class TransientFailures(string message)
{
    private int callsUntilFailure = NextGap();

    public void FailIfUnavailable()
    {
        if (--callsUntilFailure > 0)
            return;

        callsUntilFailure = NextGap();
        throw new TimeoutException(message);
    }

    private static int NextGap() => Random.Shared.Next(2, 5);
}
