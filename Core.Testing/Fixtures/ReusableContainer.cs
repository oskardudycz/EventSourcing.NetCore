using System.Security.Cryptography;
using System.Text;
using DotNet.Testcontainers.Containers;

namespace Core.Testing.Fixtures;

public static class ReusableContainer
{
    public static async Task StartAsync(
        IContainer container,
        string reuseId,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reuseId);

        var lockId = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(reuseId)));
        var lockPath = Path.Combine(Path.GetTempPath(), $"EventSourcing.NetCore.container.{lockId}.lock");

        await using var startupLock = await AcquireStartupLockAsync(lockPath, cancellationToken).ConfigureAwait(false);
        await container.StartAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task<FileStream> AcquireStartupLockAsync(string lockPath, CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        using var waiting = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

        try
        {
            while (true)
            {
                waiting.Token.ThrowIfCancellationRequested();

                try
                {
                    return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                }
                catch (IOException exception) when (IsLockContention(exception))
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), waiting.Token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Timed out waiting for the container startup lock '{lockPath}'.", exception);
        }
    }

    private static bool IsLockContention(IOException exception) =>
        // Windows reports ERROR_SHARING_VIOLATION; Unix reports the native EWOULDBLOCK errno.
        exception.HResult == unchecked((int)0x80070020)
        || OperatingSystem.IsLinux() && exception.HResult == 11
        || OperatingSystem.IsMacOS() && exception.HResult == 35;
}
