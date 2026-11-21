using OptimisticConcurrency.Core;
using Shouldly;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.Version1_Aggregates.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;

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
        await InParallel(Enumerable.Range(0, 10).Select(_ => (Func<ValueTask>)(() => groupCheckoutFacade.InitiateGroupCheckout(command))));

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now));
    }

    [Fact]
    public async Task GuestCheckoutCompletionsRecordedConcurrently_CompleteGroupCheckout()
    {
        // Given
        var guestStays = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();
        var groupCheckoutId = await InitiateGroupCheckout(guestStays);

        // When
        await InParallel(guestStays.Select(guestStayAccountId => (Func<ValueTask>)(() =>
            groupCheckoutFacade.RecordGuestCheckoutCompletion(
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayAccountId, now)
            ))
        ));

        // Then
        publishedMessages.ShouldReceiveMessagesInAnyOrder([
            ..guestStays.Select(guestStayAccountId => new GuestCheckoutCompletionRecorded(groupCheckoutId, guestStayAccountId, now)),
            new GroupCheckoutCompleted(groupCheckoutId, guestStays, now)
        ]);
    }

    [Fact]
    public async Task GuestCheckoutCompletionsAndFailureRecordedConcurrently_FailGroupCheckout()
    {
        // Given
        var guestStays = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid()).ToArray();
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
        publishedMessages.ShouldReceiveMessagesInAnyOrder([
            new GuestCheckoutFailureRecorded(groupCheckoutId, failedGuestStay, now),
            ..completedGuestStays.Select(guestStayAccountId => new GuestCheckoutCompletionRecorded(groupCheckoutId, guestStayAccountId, now)),
            new GroupCheckoutFailed(groupCheckoutId, completedGuestStays, [failedGuestStay], now)
        ]);
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

    private static Task InParallel(IEnumerable<Func<ValueTask>> handlers)
    {
        var calls = handlers.ToArray();
        var ready = 0;
        var startGate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        return Task.WhenAll(calls.Select(async handle =>
        {
            if (Interlocked.Increment(ref ready) == calls.Length)
                startGate.SetResult();

            await startGate.Task;
            await handle();
        }).ToArray());
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GroupCheckoutFacade groupCheckoutFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public OptimisticConcurrencyTests()
    {
        groupCheckoutFacade = new GroupCheckoutFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);
    }
}
