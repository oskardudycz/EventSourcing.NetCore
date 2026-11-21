using OptimisticConcurrency.Core;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.Version2_ImmutableEntities.GroupCheckouts;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;

public class IdempotencyTests
{
    [Fact]
    public async Task GroupCheckoutIsInitiatedOnceForTheSameId()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);
        // And
        await groupCheckoutFacade.InitiateGroupCheckout(command, TestContext.Current.CancellationToken);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command, TestContext.Current.CancellationToken);

        // Then

        publishedMessages.ShouldReceiveSingleMessage(new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays,
            now));
    }

    [Fact]
    public async Task GroupCheckoutRecordsOnceGuestCheckoutCompletion()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        // And
        await groupCheckoutFacade.InitiateGroupCheckout(new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays,
            now.AddDays(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();


        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now);
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command, TestContext.Current.CancellationToken);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command, TestContext.Current.CancellationToken);

        // Then

        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckoutCompleted(groupCheckoutId, guestStays[0], now));
    }

    [Fact]
    public async Task GroupCheckoutRecordsOnceGuestCheckoutFailure()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        // And
        await groupCheckoutFacade.InitiateGroupCheckout(new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays,
            now.AddDays(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();


        var command = new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[0], now);
        await groupCheckoutFacade.RecordGuestCheckoutFailure(command, TestContext.Current.CancellationToken);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutFailure(command, TestContext.Current.CancellationToken);

        // Then

        publishedMessages.ShouldReceiveSingleMessage(
            new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStays[0], now));
    }

    [Fact]
    public async Task GroupCheckoutCompletesOnceWhenTheLastGuestCheckoutCompletionIsRecordedTwice()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid()];
        await groupCheckoutFacade.InitiateGroupCheckout(new InitiateGroupCheckout(groupCheckoutId, Guid.NewGuid(), guestStays, now), TestContext.Current.CancellationToken);
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[1], now);
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command, TestContext.Current.CancellationToken);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GuestCheckoutCompleted(groupCheckoutId, guestStays[1], now),
                new GroupCheckoutCompleted(groupCheckoutId, guestStays, now)
            ]
        );
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GroupCheckOutFacade groupCheckoutFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public IdempotencyTests()
    {
        groupCheckoutFacade = new GroupCheckOutFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);
    }
}
