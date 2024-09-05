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

public class GroupCheckOut
{
    public required Guid Id { get; init; }
    public required Dictionary<Guid, CheckoutStatus> GuestStayCheckouts { get; init;  }
    public CheckoutStatus Status { get; set; } = CheckoutStatus.Initiated;
}

public enum CheckoutStatus
{
    Initiated,
    Completed,
    Failed
}
