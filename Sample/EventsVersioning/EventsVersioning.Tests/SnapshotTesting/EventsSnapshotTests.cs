using System.Runtime.CompilerServices;

namespace EventsVersioning.Tests.SnapshotTesting;

public class EventsSnapshotTests
{
    private record ShoppingCartConfirmed(
        Guid ShoppingCartId,
        string? ClientId,
        DateTimeOffset ConfirmedAt
    );

    [Fact]
    public Task ShoppingCartConfirmed_WithCompleteData_IsCompatible()
    {
        var @event = new ShoppingCartConfirmed(Guid.CreateVersion7(), "Oskar Dudycz", DateTimeOffset.UtcNow);
        return Verify(@event);
    }

    [Fact]
    public Task ShoppingCartConfirmed_WithOnlyRequiredData_IsCompatible()
    {
        var @event = new ShoppingCartConfirmed(Guid.CreateVersion7(), null, DateTimeOffset.UtcNow);
        return Verify(@event);
    }
}
// note this is optional, if you really need to
// This is just showing that you can
public static class StaticSettingsUsage
{
    [ModuleInitializer]
    public static void Initialize() =>
        VerifierSettings.AddScrubber(text => text.Replace("Oskar Dudycz", "anonymised"));
}
