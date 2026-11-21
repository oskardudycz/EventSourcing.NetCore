using OptimisticConcurrency.Core;
using Shouldly;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class OptimisticConcurrencyTests
{
    [Fact]
    public async Task GuestCheckedInConcurrently_IsCheckedInOnce()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var command = new CheckInGuest(guestStayAccountId, now);

        // When
        await InParallel(Enumerable.Range(0, 10).Select(_ => (Func<ValueTask>)(() => guestStayFacade.CheckInGuest(command))));

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedIn(guestStayAccountId, now));
    }

    [Fact]
    public async Task ChargesRecordedConcurrently_AllCountTowardsBalance()
    {
        // Given
        var guestStayAccountId = await CheckInGuest();
        decimal[] amounts = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];

        // When
        await InParallel(amounts.Select(amount => (Func<ValueTask>)(() =>
            guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, Guid.NewGuid(), amount, now))
        )));

        // Then
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayAccountId, Guid.NewGuid(), amounts.Sum(), now), TestContext.Current.CancellationToken);
        publishedMessages.Reset();

        await guestStayFacade.CheckOutGuest(new CheckOutGuest(guestStayAccountId, now), TestContext.Current.CancellationToken);

        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayAccountId, now));
    }

    [Fact]
    public async Task PaymentsRecordedConcurrently_AllCountTowardsBalance()
    {
        // Given
        var guestStayAccountId = await CheckInGuest();
        decimal[] amounts = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, Guid.NewGuid(), amounts.Sum(), now), TestContext.Current.CancellationToken);

        // When
        await InParallel(amounts.Select(amount => (Func<ValueTask>)(() =>
            guestStayFacade.RecordPayment(new RecordPayment(guestStayAccountId, Guid.NewGuid(), amount, now))
        )));

        // Then
        publishedMessages.Reset();

        await guestStayFacade.CheckOutGuest(new CheckOutGuest(guestStayAccountId, now), TestContext.Current.CancellationToken);

        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayAccountId, now));
    }

    [Fact]
    public async Task ChargeRecordedConcurrentlyWithCheckout_DoesNotLetGuestCheckOutWithUnsettledBalance()
    {
        // Given
        var guestStayAccountIds = await CheckInGuests(10);

        // When
        await InParallel(guestStayAccountIds.SelectMany(guestStayAccountId => new Func<ValueTask>[]
        {
            () => guestStayFacade.CheckOutGuest(new CheckOutGuest(guestStayAccountId, now)),
            () => guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, Guid.NewGuid(), 100, now))
        }), ignoring: typeof(InvalidOperationException));

        // Then
        Type[] checkedOutBeforeCharge = [typeof(GuestCheckedOut)];
        Type[] chargedBeforeCheckout = [typeof(ChargeRecorded), typeof(GuestCheckoutFailed)];

        foreach (var guestStayAccountId in guestStayAccountIds)
        {
            var publishedTypes = publishedMessages.Published
                .Where(message => GuestStayAccountIdOf(message) == guestStayAccountId)
                .Select(message => message.GetType())
                .ToArray();

            (
                publishedTypes.ToHashSet().SetEquals(checkedOutBeforeCharge) && publishedTypes.Length == checkedOutBeforeCharge.Length ||
                publishedTypes.ToHashSet().SetEquals(chargedBeforeCheckout) && publishedTypes.Length == chargedBeforeCheckout.Length
            ).ShouldBeTrue();
        }
    }

    private async Task<Guid> CheckInGuest()
    {
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)));
        publishedMessages.Reset();

        return guestStayAccountId;
    }

    private async Task<Guid[]> CheckInGuests(int count)
    {
        var guestStayAccountIds = new List<Guid>();
        for (var i = 0; i < count; i++)
            guestStayAccountIds.Add(await CheckInGuest());

        return guestStayAccountIds.ToArray();
    }

    private static Guid GuestStayAccountIdOf(object message) =>
        message switch
        {
            ChargeRecorded charge => charge.GuestStayAccountId,
            GuestCheckedOut checkedOut => checkedOut.GuestStayAccountId,
            GuestCheckoutFailed failed => failed.GuestStayAccountId,
            _ => throw new InvalidOperationException($"Unexpected message: {message.GetType().Name}")
        };

    private static Task InParallel(IEnumerable<Func<ValueTask>> handlers, Type? ignoring = null)
    {
        var calls = handlers.ToArray();
        var ready = 0;
        var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        return Task.WhenAll(calls.Select(async handle =>
        {
            if (Interlocked.Increment(ref ready) == calls.Length)
                startGate.SetResult();

            await startGate.Task;

            try
            {
                await handle();
            }
            catch (Exception exc) when (exc.GetType() == ignoring)
            {
            }
        }).ToArray());
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayAccountFacade guestStayFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public OptimisticConcurrencyTests()
    {
        guestStayFacade = new GuestStayAccountFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);
    }
}
