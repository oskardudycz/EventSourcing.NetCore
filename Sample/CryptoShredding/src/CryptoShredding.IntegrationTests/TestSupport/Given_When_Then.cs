namespace CryptoShredding.IntegrationTests.TestSupport;

public abstract class Given_WhenAsync_Then_Test
    : IDisposable
{
    protected Given_WhenAsync_Then_Test()
    {
        Task.Run((Func<Task>) (async () => await this.SetupAsync())).Wait();
    }

    private async Task SetupAsync()
    {
        await Given();
        await When();
    }

    protected abstract Task Given();

    protected abstract Task When();

    public void Dispose()
    {
        Cleanup();
    }

    protected virtual void Cleanup()
    {
    }
}
