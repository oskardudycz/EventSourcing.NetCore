namespace BusinessProcesses.ToDoList.GroupCheckouts;

public abstract record GroupCheckoutEvent
{
    public record GroupCheckoutInitiated(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayAccountIds,
        DateTimeOffset InitiatedAt
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

public record GroupCheckout(
    Guid Id,
    Dictionary<Guid, CheckoutStatus> GuestStayCheckouts,
    DateTimeOffset UpdatedAt
)
{
    public static GroupCheckout Initiated(Guid id, Guid[] guestStayAccountIds, DateTimeOffset initiatedAt) =>
        new(id, guestStayAccountIds.Distinct().ToDictionary(guestStayAccountId => guestStayAccountId, _ => CheckoutStatus.Initiated), initiatedAt);

    public GroupCheckout WithCheckout(Guid guestStayAccountId, CheckoutStatus status, DateTimeOffset at) =>
        this with
        {
            GuestStayCheckouts = new Dictionary<Guid, CheckoutStatus>(GuestStayCheckouts) { [guestStayAccountId] = status },
            UpdatedAt = at
        };

    public bool AreAnyOngoingCheckouts() =>
        GuestStayCheckouts.Values.Any(status => status is CheckoutStatus.Initiated);

    public bool AreAnyFailedCheckouts() =>
        GuestStayCheckouts.Values.Any(status => status is CheckoutStatus.Failed);

    public Guid[] CheckoutsWith(CheckoutStatus status) =>
        GuestStayCheckouts
            .Where(pair => pair.Value == status)
            .Select(pair => pair.Key)
            .ToArray();
}

public enum CheckoutStatus
{
    Initiated,
    Completed,
    Failed
}
