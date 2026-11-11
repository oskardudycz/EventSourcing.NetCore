using FluentAssertions;
using OptimisticConcurrency.Core;
using OptimisticConcurrency.Version1_Aggregates.GroupCheckouts;
using OptimisticConcurrency.Version1_Aggregates.GuestStayAccounts;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;
using GroupCheckoutCommand = OptimisticConcurrency.Version1_Aggregates.GroupCheckouts.GroupCheckoutCommand;

namespace OptimisticConcurrency.Version1_Aggregates;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class OptimisticConcurrencyTests
{
    [Fact]
    public async Task GroupCheckoutInitiatedConcurrently_IsInitiatedOnce()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await InParallel(Enumerable.Range(0, 5).Select(_ => (Func<ValueTask>)(() => groupCheckoutFacade.InitiateGroupCheckout(command))));

        // Then
        publishedMessages.ShouldReceiveMessages([
            new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now)
        ]);
    }

    [Fact]
    public async Task GuestCheckoutCompletionsRecordedConcurrently_CompleteGroupCheckout()
    {
        // Given
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var groupCheckoutId = await InitiateGroupCheckout(guestStays);

        // When
        await InParallel(guestStays.Select(guestStayId => (Func<ValueTask>)(() =>
            groupCheckoutFacade.RecordGuestCheckoutCompletion(
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayId, now)
            ))
        ));

        // Then
        publishedMessages.ShouldReceiveMessages([
            ..guestStays.Select(guestStayId => new GuestCheckoutCompleted(groupCheckoutId, guestStayId, now)),
            new GroupCheckoutCompleted(groupCheckoutId, guestStays, now)
        ]);
    }

    [Fact]
    public async Task GuestCheckoutCompletionsAndFailureRecordedConcurrently_FailGroupCheckout()
    {
        // Given
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var failedGuestStay = guestStays[0];
        var completedGuestStays = guestStays[1..];
        var groupCheckoutId = await InitiateGroupCheckout(guestStays);

        // When
        await InParallel([
            () => groupCheckoutFacade.RecordGuestCheckoutFailure(
                new RecordGuestCheckoutFailure(groupCheckoutId, failedGuestStay, now)
            ),
            ..completedGuestStays.Select(guestStayId => (Func<ValueTask>)(() =>
                groupCheckoutFacade.RecordGuestCheckoutCompletion(
                    new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayId, now)
                )
            ))
        ]);

        // Then
        publishedMessages.ShouldReceiveMessages([
            new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, failedGuestStay, now),
            ..completedGuestStays.Select(guestStayId => new GuestCheckoutCompleted(groupCheckoutId, guestStayId, now)),
            new GroupCheckoutFailed(groupCheckoutId, completedGuestStays, [failedGuestStay], now)
        ]);
    }

    [Fact]
    public async Task ChargesRecordedConcurrently_AllCountTowardsBalance()
    {
        // Given
        var guestStayId = await CheckInGuest();
        decimal[] amounts = [10, 20, 30, 40, 50];

        await InParallel(amounts.Select(amount => (Func<ValueTask>)(() =>
            guestStayFacade.RecordCharge(new RecordCharge(guestStayId, Guid.NewGuid(), amount, now))
        )));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayId, Guid.NewGuid(), amounts.Sum(), now));
        publishedMessages.Reset();

        // When
        await guestStayFacade.CheckOutGuest(new CheckOutGuest(guestStayId, now));

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayId, now));
    }

    [Fact]
    public async Task ChargeRecordedConcurrentlyWithCheckout_DoesNotLetGuestCheckOutWithUnsettledBalance()
    {
        // Given
        var guestStayId = await CheckInGuest();

        // When
        await InParallel([
            () => guestStayFacade.CheckOutGuest(new CheckOutGuest(guestStayId, now)),
            () => guestStayFacade.RecordCharge(new RecordCharge(guestStayId, Guid.NewGuid(), 100, now))
        ], ignoring: typeof(InvalidOperationException));

        // Then
        var publishedTypes = publishedMessages.Published.Select(message => message.GetType()).ToArray();
        Type[] checkedOutBeforeCharge = [typeof(GuestCheckedOut)];
        Type[] chargedBeforeCheckout = [typeof(ChargeRecorded), typeof(GuestStayAccountEvent.GuestCheckOutFailed)];

        publishedTypes.Should().Match<Type[]>(types =>
            types.ToHashSet().SetEquals(checkedOutBeforeCharge) && types.Length == checkedOutBeforeCharge.Length ||
            types.ToHashSet().SetEquals(chargedBeforeCheckout) && types.Length == chargedBeforeCheckout.Length
        );
    }

    private async Task<Guid> InitiateGroupCheckout(Guid[] guestStays)
    {
        var groupCheckoutId = Guid.NewGuid();
        await groupCheckoutFacade.InitiateGroupCheckout(
            new InitiateGroupCheckout(groupCheckoutId, Guid.NewGuid(), guestStays, now.AddDays(-1))
        );
        publishedMessages.Reset();

        return groupCheckoutId;
    }

    private async Task<Guid> CheckInGuest()
    {
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        publishedMessages.Reset();

        return guestStayId;
    }

    private static Task InParallel(IEnumerable<Func<ValueTask>> handlers, Type? ignoring = null) =>
        Task.WhenAll(handlers.Select(async handle =>
        {
            try
            {
                await handle();
            }
            catch (Exception exc) when (exc.GetType() == ignoring)
            {
            }
        }).ToArray());

    private readonly Database database = new();
    private readonly EventBus eventBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayFacade guestStayFacade;
    private readonly GroupCheckOutFacade groupCheckoutFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public OptimisticConcurrencyTests()
    {
        guestStayFacade = new GuestStayFacade(database, eventBus);
        groupCheckoutFacade = new GroupCheckOutFacade(database, eventBus);

        eventBus.Use(publishedMessages.Catch);
    }
}
