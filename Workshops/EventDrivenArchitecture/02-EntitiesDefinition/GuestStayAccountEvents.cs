namespace EntitiesDefinition;

public abstract record GuestStayAccountEvent
{
    public record GuestCheckedIn(
        Guid GuestStayAccountId,
        DateTimeOffset OpenedAt
    ): GuestStayAccountEvent;

    public record ChargeRecorded(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record PaymentRecorded(
        Guid GuestStayAccountId,
        decimal Amount,
        DateTimeOffset RecordedAt
    ): GuestStayAccountEvent;

    public record GuestCheckedOut(
        Guid GuestStayAccountId,
        DateTimeOffset CompletedAt,
        Guid? GroupCheckoutId = null
    ): GuestStayAccountEvent;

    public record GuestCheckoutFailed(
        Guid GuestStayAccountId,
        GuestCheckoutFailed.FailureReason Reason,
        DateTimeOffset FailedAt,
        Guid? GroupCheckoutId = null
    ): GuestStayAccountEvent
    {
        public enum FailureReason
        {
            InvalidState,
            BalanceNotSettled
        }
    }

    private GuestStayAccountEvent() { }
}
