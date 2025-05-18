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
        Guid GroupCheckOutId,
        Guid ClerkId,
        Guid[] GuestStayIds,
        DateTimeOffset InitiatedAt
    ): GroupCheckoutEvent;

    public record GuestCheckOutCompleted(
        Guid GroupCheckOutId,
        Guid GuestStayId,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GuestCheckOutFailed(
        Guid GroupCheckOutId,
        Guid GuestStayId,
        DateTimeOffset FailedAt
    ): GroupCheckoutEvent;

    public record GroupCheckOutCompleted(
        Guid GroupCheckOutId,
        Guid[] CompletedCheckouts,
        DateTimeOffset CompletedAt
    ): GroupCheckoutEvent;

    public record GroupCheckoutFailed(
        Guid GroupCheckOutId,
        Guid[] CompletedCheckouts,
        Guid[] FailedCheckouts,
        DateTimeOffset FailedAt
    ): GroupCheckoutEvent;

    private GroupCheckoutEvent() { }
}

public record GroupCheckOut(
    Guid Id,
    Dictionary<Guid, CheckoutStatus> GuestStayCheckouts,
    CheckoutStatus Status = CheckoutStatus.Initiated
)
{
    public static ProcessManagerResult[] Handle(InitiateGroupCheckout command)
    {
        var (groupCheckoutId, clerkId, guestStayIds, initiatedAt) = command;

        return
        [
            Publish(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayIds, initiatedAt)),
            ..guestStayIds.Select(guestAccountId =>
                Send(new CheckOutGuest(guestAccountId, initiatedAt, groupCheckoutId))
            )
        ];
    }

    public ProcessManagerResult[] On(GuestStayAccountEvent.GuestCheckedOut @event)
    {
        var (guestStayId, checkedOutAt, _) = @event;

        // We could consider even not publishing this event, but storing the one from guest stay account
        var guestCheckoutCompleted = new GuestCheckOutCompleted(Id, guestStayId, checkedOutAt);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Completed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [Publish(guestCheckoutCompleted)]
            : [Publish(guestCheckoutCompleted), Publish(Finalize(guestStayCheckouts, checkedOutAt))];
    }

    public ProcessManagerResult[] On(GuestStayAccountEvent.GuestCheckOutFailed @event)
    {
        var (guestStayId, _, failedAt, _) = @event;

        var guestCheckoutFailed = new GuestCheckOutFailed(Id, guestStayId, failedAt);

        var guestStayCheckouts = GuestStayCheckouts.With(guestStayId, CheckoutStatus.Failed);

        return AreAnyOngoingCheckouts(guestStayCheckouts)
            ? [Publish(guestCheckoutFailed)]
            : [Publish(guestCheckoutFailed), Publish(Finalize(guestStayCheckouts, failedAt))];
    }

    private GroupCheckoutEvent Finalize(
        Dictionary<Guid, CheckoutStatus> guestStayCheckouts,
        DateTimeOffset now
    ) =>
        !AreAnyFailedCheckouts(guestStayCheckouts)
            ? new GroupCheckOutCompleted
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


    public GroupCheckOut Evolve(GroupCheckoutEvent @event) =>
        @event switch
        {
            GroupCheckoutInitiated initiated => this with
            {
                Id = initiated.GroupCheckOutId,
                GuestStayCheckouts = initiated.GuestStayIds.ToDictionary(id => id, _ => CheckoutStatus.Initiated),
                Status = CheckoutStatus.Initiated
            },
            GuestCheckOutCompleted guestCheckedOut => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOut.GuestStayId ? CheckoutStatus.Completed : vs.Value
                )
            },
            GuestCheckOutFailed guestCheckedOutFailed => this with
            {
                GuestStayCheckouts = GuestStayCheckouts.ToDictionary(
                    ks => ks.Key,
                    vs => vs.Key == guestCheckedOutFailed.GuestStayId ? CheckoutStatus.Failed : vs.Value
                )
            },
            GroupCheckOutCompleted => this with { Status = CheckoutStatus.Completed },
            GroupCheckoutFailed => this with { Status = CheckoutStatus.Failed },
            _ => this
        };

    public static GroupCheckOut Initial = new(default, [], default);
}

public enum CheckoutStatus
{
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
