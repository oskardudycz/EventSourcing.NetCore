using BusinessProcesses.ProcessManagers.GuestStayAccounts;
using BusinessProcesses.Sagas.Version2_ImmutableEntities.Core;

namespace BusinessProcesses.ProcessManagers.GroupCheckouts;

using static GuestStayAccountCommand;
using static GroupCheckoutEvent;
using static GroupCheckoutCommand;
using static ProcessManagerResult;

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

public record GroupCheckout(
    Guid Id,
    Dictionary<Guid, CheckoutStatus> GuestStayCheckouts,
    CheckoutStatus Status = CheckoutStatus.NotExisting
)
{
    public static ProcessManagerResult[] Handle(InitiateGroupCheckout command)
    {
        var (groupCheckoutId, clerkId, guestStayAccountIds, initiatedAt) = command;

        return
        [
            Publish(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayAccountIds, initiatedAt)),
            ..guestStayAccountIds.Select(guestAccountId =>
                Send(new CheckOutGuest(guestAccountId, initiatedAt, groupCheckoutId))
            )
        ];
    }

    public ProcessManagerResult[] On(GuestStayAccountEvent.GuestCheckedOut @event)
    {
        if (Status != CheckoutStatus.Initiated)
            return [Ignore];

        var (guestStayAccountId, checkedOutAt, _) = @event;

        // We could consider even not publishing this event, but storing the one from guest stay account
        var guestCheckoutCompleted = new GuestCheckoutCompletionRecorded(Id, guestStayAccountId, checkedOutAt);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayAccountId, CheckoutStatus.Completed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [Publish(guestCheckoutCompleted)]
            : [Publish(guestCheckoutCompleted), Publish(Finish(guestStayCheckouts, checkedOutAt))];
    }

    public ProcessManagerResult[] On(GuestStayAccountEvent.GuestCheckoutFailed @event)
    {
        if (Status != CheckoutStatus.Initiated)
            return [Ignore];

        var (guestStayAccountId, _, failedAt, _) = @event;

        var guestCheckoutFailed = new GuestCheckoutFailureRecorded(Id, guestStayAccountId, failedAt);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayAccountId, CheckoutStatus.Failed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [Publish(guestCheckoutFailed)]
            : [Publish(guestCheckoutFailed), Publish(Finish(guestStayCheckouts, failedAt))];
    }

    private GroupCheckoutEvent Finish(
        Dictionary<Guid, CheckoutStatus> guestStayCheckouts,
        DateTimeOffset now
    ) =>
        !AreAnyFailedCheckouts(guestStayCheckouts)
            ? new GroupCheckoutCompleted
            (
                Id,
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                now
            )
            : new GroupCheckoutFailed
            (
                Id,
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Completed),
                CheckoutsWith(guestStayCheckouts, CheckoutStatus.Failed),
                now
            );

    private static bool AreAnyOngoingCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
        guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Initiated);

    private static bool AreAnyFailedCheckouts(Dictionary<Guid, CheckoutStatus> guestStayCheckouts) =>
        guestStayCheckouts.Values.Any(status => status is CheckoutStatus.Failed);

    private static Guid[] CheckoutsWith(Dictionary<Guid, CheckoutStatus> guestStayCheckouts, CheckoutStatus status) =>
        guestStayCheckouts
            .Where(pair => pair.Value == status)
            .Select(pair => pair.Key)
            .ToArray();


    public GroupCheckout Evolve(GroupCheckoutEvent @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated => this with
            {
                Id = initiated.GroupCheckoutId,
                GuestStayCheckouts = initiated.GuestStayAccountIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated),
                Status = CheckoutStatus.Initiated
            },
            GuestCheckoutCompletionRecorded guestCheckedOut => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOut.GuestStayAccountId ? CheckoutStatus.Completed : vs.Value
                )
            },
            GuestCheckoutFailureRecorded guestCheckedOutFailed => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOutFailed.GuestStayAccountId ? CheckoutStatus.Failed : vs.Value
                )
            },
            GroupCheckoutCompleted => this with { Status = CheckoutStatus.Completed },
            GroupCheckoutFailed => this with { Status = CheckoutStatus.Failed },
            _ => this
        };

    public static GroupCheckout Initial = new(default, [], CheckoutStatus.NotExisting);
}

public enum CheckoutStatus
{
    NotExisting,
    Initiated,
    Completed,
    Failed
}

public abstract record ProcessManagerResult
{
    public record Command(object Message): ProcessManagerResult;
    public record Command<T>(T TypedMessage): Command(TypedMessage) where T : notnull;

    public record Event(object Message): ProcessManagerResult;
    public record Event<T>(T TypedMessage): Event(TypedMessage) where T : notnull;

    public record None: ProcessManagerResult;

    public static Command<T> Send<T>(T command) where T : notnull => new(command);

    public static Event<T> Publish<T>(T @event) where T : notnull => new(@event);

    public static readonly None Ignore = new();
}
