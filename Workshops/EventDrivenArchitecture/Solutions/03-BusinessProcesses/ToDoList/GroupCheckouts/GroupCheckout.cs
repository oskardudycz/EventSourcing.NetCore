namespace BusinessProcesses.ToDoList.GroupCheckouts;

public abstract record GroupCheckoutEvent
{
    public record GroupCheckoutInitiated(
        Guid GroupCheckoutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
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

public record GroupCheckOut(
    Guid Id,
    Dictionary<Guid, CheckoutStatus> GuestStayCheckouts,
    DateTimeOffset UpdatedAt
)
{
    public static GroupCheckOut Initiated(Guid id, Guid[] guestStayIds, DateTimeOffset initiatedAt) =>
        new(id, guestStayIds.Distinct().ToDictionary(guestStayId => guestStayId, _ => CheckoutStatus.Initiated), initiatedAt);

    public GroupCheckOut WithCheckout(Guid guestStayId, CheckoutStatus status, DateTimeOffset at) =>
        this with
        {
            GuestStayCheckouts = new Dictionary<Guid, CheckoutStatus>(GuestStayCheckouts) { [guestStayId] = status },
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
