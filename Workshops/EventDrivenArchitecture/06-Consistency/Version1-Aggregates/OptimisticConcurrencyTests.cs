using Shouldly;
using Consistency.Core;
using Consistency.Version1_Aggregates.GroupCheckouts;
using Consistency.Version1_Aggregates.GuestStayAccounts;
using Xunit;
using Database = Consistency.Core.Database;
using GroupCheckoutCommand = Consistency.Version1_Aggregates.GroupCheckouts.GroupCheckoutCommand;

namespace Consistency.Version1_Aggregates;

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
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now));
    }

    [Fact]
    public async Task GuestCheckoutCompletionsRecordedConcurrently_CompleteGroupCheckout()
    {
        // Given
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var groupCheckoutId = await InitiateGroupCheckout(guestStays);

        // When
        await InParallel(guestStays.Select(guestStayAccountId => (Func<ValueTask>)(() =>
            groupCheckoutFacade.RecordGuestCheckoutCompletion(
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayAccountId, now)
            ))
        ));

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveMessagesInAnyOrder([
            ..guestStays.Select(guestStayAccountId => new GuestCheckoutCompletionRecorded(groupCheckoutId, guestStayAccountId, now)),
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
            ..completedGuestStays.Select(guestStayAccountId => (Func<ValueTask>)(() =>
                groupCheckoutFacade.RecordGuestCheckoutCompletion(
                    new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayAccountId, now)
                )
            ))
        ]);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveMessagesInAnyOrder([
            new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, failedGuestStay, now),
            ..completedGuestStays.Select(guestStayAccountId => new GuestCheckoutCompletionRecorded(groupCheckoutId, guestStayAccountId, now)),
            new GroupCheckoutFailed(groupCheckoutId, completedGuestStays, [failedGuestStay], now)
        ]);
    }

    [Fact]
    public async Task ChargesRecordedConcurrently_AllCountTowardsBalance()
    {
        // Given
        var guestStayAccountId = await CheckInGuest();
        decimal[] amounts = [10, 20, 30, 40, 50];

        await InParallel(amounts.Select(amount => (Func<ValueTask>)(() =>
            guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, amount, now))
        )));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayAccountId, amounts.Sum(), now), TestContext.Current.CancellationToken);
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.Reset();

        // When
        await guestStayFacade.CheckOutGuest(new CheckOutGuest(guestStayAccountId, now), TestContext.Current.CancellationToken);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayAccountId, now));
    }

    [Fact]
    public async Task ChargeRecordedConcurrentlyWithCheckout_DoesNotLetGuestCheckOutWithUnsettledBalance()
    {
        // Given
        var guestStayAccountId = await CheckInGuest();

        // When
        await InParallel([
            () => guestStayFacade.CheckOutGuest(new CheckOutGuest(guestStayAccountId, now)),
            () => guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, 100, now))
        ], ignoring: typeof(InvalidOperationException));

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        var publishedTypes = publishedMessages.Published.Select(message => message.GetType()).ToArray();
        Type[] checkedOutBeforeCharge = [typeof(GuestCheckedOut)];
        Type[] chargedBeforeCheckout = [typeof(ChargeRecorded), typeof(GuestStayAccountEvent.GuestCheckoutFailed)];

        (
            publishedTypes.ToHashSet().SetEquals(checkedOutBeforeCharge) && publishedTypes.Length == checkedOutBeforeCharge.Length ||
            publishedTypes.ToHashSet().SetEquals(chargedBeforeCheckout) && publishedTypes.Length == chargedBeforeCheckout.Length
        ).ShouldBeTrue();
    }

    private async Task<Guid> InitiateGroupCheckout(Guid[] guestStays)
    {
        var groupCheckoutId = Guid.NewGuid();
        await groupCheckoutFacade.InitiateGroupCheckout(
            new InitiateGroupCheckout(groupCheckoutId, Guid.NewGuid(), guestStays, now.AddDays(-1))
        );
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();

        return groupCheckoutId;
    }

    private async Task<Guid> CheckInGuest()
    {
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)));
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();

        return guestStayAccountId;
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
    private readonly MessageBus messageBus = new();
    private readonly BackgroundJobs backgroundJobs = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayAccountFacade guestStayFacade;
    private readonly GroupCheckoutFacade groupCheckoutFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public OptimisticConcurrencyTests()
    {
        guestStayFacade = new GuestStayAccountFacade(database, messageBus, backgroundJobs);
        groupCheckoutFacade = new GroupCheckoutFacade(database, messageBus, backgroundJobs);

        messageBus.Use(publishedMessages.Catch);
    }
}
