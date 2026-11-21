namespace EntitiesDefinition;

public abstract record GroupCheckoutEvent
{
    public record GroupCheckoutInitiated(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayAccountIds,
        DateTimeOffset InitiatedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutCompletionRecorded(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GuestCheckoutFailureRecorded(
        Guid GroupCheckoutId,
        Guid GuestStayAccountId,
        DateTimeOffset FailedAt
    ): GroupCheckoutEvent;

    public record GroupCheckoutCompleted(
        Guid GroupCheckoutId,
        Guid[] CompletedCheckouts,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GroupCheckoutFailed(
        Guid GroupCheckoutId,
        Guid[] CompletedCheckouts,
        Guid[] FailedCheckouts,
        DateTimeOffset FailedAt
    ): GroupCheckoutEvent;

    private GroupCheckoutEvent() { }
}
