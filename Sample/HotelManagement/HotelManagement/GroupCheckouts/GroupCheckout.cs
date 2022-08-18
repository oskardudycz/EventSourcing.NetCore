namespace HotelManagement.GroupCheckouts;

public record GroupCheckoutInitiated(
    Guid GroupCheckoutId,
    Guid ClerkId,
    Guid[] GuestStayIds,
    DateTimeOffset InitiatedAt
);

public record GuestCheckoutsInitiated(
    Guid GroupCheckoutId,
    Guid ClerkId,
    Guid[] GuestStayIds,
    DateTimeOffset InitiatedAt
);

public record GuestCheckoutCompleted(
    Guid GroupCheckoutId,
    Guid GuestStayId,
    DateTimeOffset CompletedAt
);

public record GuestCheckoutFailed(
    Guid GroupCheckoutId,
    Guid GuestStayId,
    DateTimeOffset CompletedAt
);

public record GroupCheckoutCompleted(
    Guid GroupCheckoutId,
    Guid[] CompletedCheckouts,
    DateTimeOffset CompletedAt
);

public record GroupCheckoutFailed(
    Guid GroupCheckoutId,
    Guid[] CompletedCheckouts,
    Guid[] FailedCheckouts,
    DateTimeOffset FailedAt
);
