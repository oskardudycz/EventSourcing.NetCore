using System.Runtime.CompilerServices;

namespace EventsVersioning.Tests.SnapshotTesting;

public class EventsSnapshotTests
{
    public record PaymentRecorded(
        string GuestStayAccountId,
        decimal Amount,
        DateTimeOffset RecordedAt,
        string? ClerkId = null
    );

    [Fact]
    public Task ShoppingCartConfirmed_WithCompleteData_IsCompatible()
    {
        var @event = new PaymentRecorded(
            Guid.NewGuid().ToString(),
            292.333m,
            DateTimeOffset.UtcNow,
            "Oskar Dudycz"
        );
        return Verify(@event);
    }

    [Fact]
    public Task ShoppingCartConfirmed_WithOnlyRequiredData_IsCompatible()
    {
        var @event = new PaymentRecorded(
            Guid.NewGuid().ToString(),
            292.333m,
            DateTimeOffset.UtcNow
        );
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
