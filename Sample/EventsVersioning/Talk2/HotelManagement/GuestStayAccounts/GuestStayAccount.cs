namespace HotelManagement.GuestStayAccounts;

public record GuestStayAccount(
    string Id,
    decimal Balance = 0,
    GuestStayAccountStatus Status = GuestStayAccountStatus.Opened
)
{
    public bool IsSettled => Balance == 0;

    public static string GuestStayAccountId(string guestStayId, string roomId, DateOnly checkInDate) =>
        $"{guestStayId}:{roomId}:{checkInDate:yyyy-MM-dd}";


    public static GuestStayAccount Evolve(GuestStayAccount state, object @event) =>
        @event switch
        {
            GuestCheckedIn checkedIn => state with
            {
                Id = checkedIn.GuestStayAccountId, Status = GuestStayAccountStatus.Opened
            },
            ChargeRecorded charge => state with { Balance = state.Balance - charge.Amount },
            PaymentRecorded payment => state with { Balance = state.Balance + payment.Amount },
            GuestCheckedOut => state with { Status = GuestStayAccountStatus.CheckedOut },
            GuestCheckoutFailed => state,
            _ => state
        };

    public static readonly GuestStayAccount Initial = new("Unknown", -1, GuestStayAccountStatus.NotExisting);
}

public enum GuestStayAccountStatus
{
    NotExisting = 0,
    Opened = 1,
    CheckedOut = 2
}

public record GuestCheckedIn(
    string GuestStayAccountId,
    string GuestStayId,
    string RoomId,
    string ClerkId,
    DateTimeOffset CheckedInAt
);

public record ChargeRecorded(
    string GuestStayAccountId,
    decimal Amount,
    DateTimeOffset RecordedAt
);

public record PaymentRecorded(
    string GuestStayAccountId,
    decimal Amount,
    DateTimeOffset RecordedAt
);

public record GuestCheckedOut(
    string GuestStayAccountId,
    string ClerkId,
    DateTimeOffset CheckedOutAt
);

public record GuestCheckoutFailed(
    string GuestStayAccountId,
    string ClerkId,
    GuestCheckoutFailed.FailureReason Reason,
    DateTimeOffset FailedAt
)
{
    public enum FailureReason
    {
        NotOpened,
        BalanceNotSettled
    }
}
