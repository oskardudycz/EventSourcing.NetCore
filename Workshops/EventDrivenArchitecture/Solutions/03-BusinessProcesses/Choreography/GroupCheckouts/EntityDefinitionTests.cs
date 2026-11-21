using BusinessProcesses.Core;
using BusinessProcesses.Choreography.GuestStayAccounts;
using Xunit;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GroupCheckoutCommand;
using static GuestStayAccountCommand;
using static GroupCheckoutsConfig;

public class EntityDefinitionTests
{
    [Fact]
    public async Task InitiatingGroupCheckout_Succeeds()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStayAccountIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStayAccountIds, now);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayAccountIds, now),
                new CheckOutGuest(guestStayAccountIds[0], now, groupCheckoutId),
                new CheckOutGuest(guestStayAccountIds[1], now, groupCheckoutId),
                new CheckOutGuest(guestStayAccountIds[2], now, groupCheckoutId),
            ]
        );
    }

    [Fact]
    public async Task RecordingGuestCheckoutCompletion_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(guestStayAccountId, Guid.NewGuid(), Guid.NewGuid());
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(guestStayAccountId, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(@event, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, guestStayAccountId, now),
            ]
        );
    }

    [Fact]
    public async Task RecordingGuestCheckoutFailure_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(guestStayAccountId, Guid.NewGuid(), Guid.NewGuid());
        // And
        var @event = new GuestStayAccountEvent.GuestCheckoutFailed(guestStayAccountId, GuestStayAccountEvent.GuestCheckoutFailed.FailureReason.BalanceNotSettled, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutFailure(@event, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckoutFailureRecorded(groupCheckoutId, guestStayAccountId, now),
            ]
        );
    }

    [Fact]
    public async Task RecordingLastGuestCheckoutCompletion_CompletesGroupCheckout()
    {
        // Given
        var completedGuestStayAccountId = Guid.NewGuid();
        var lastGuestStayAccountId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(completedGuestStayAccountId, lastGuestStayAccountId);
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(new GuestStayAccountEvent.GuestCheckedOut(completedGuestStayAccountId, now, groupCheckoutId), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayAccountId, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(@event, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, lastGuestStayAccountId, now),
                new GroupCheckoutEvent.GroupCheckoutCompleted(groupCheckoutId, [completedGuestStayAccountId, lastGuestStayAccountId], now),
            ]
        );
    }

    [Fact]
    public async Task RecordingLastGuestCheckoutAfterFailure_FailsGroupCheckout()
    {
        // Given
        var failedGuestStayAccountId = Guid.NewGuid();
        var lastGuestStayAccountId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(failedGuestStayAccountId, lastGuestStayAccountId);
        await groupCheckoutFacade.RecordGuestCheckoutFailure(new GuestStayAccountEvent.GuestCheckoutFailed(failedGuestStayAccountId, GuestStayAccountEvent.GuestCheckoutFailed.FailureReason.BalanceNotSettled, now, groupCheckoutId), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayAccountId, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(@event, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckoutCompletionRecorded(groupCheckoutId, lastGuestStayAccountId, now),
                new GroupCheckoutEvent.GroupCheckoutFailed(groupCheckoutId, [lastGuestStayAccountId], [failedGuestStayAccountId], now),
            ]
        );
    }

    private async Task<Guid> InitiateGroupCheckout(params Guid[] guestStayAccountIds)
    {
        var groupCheckoutId = Guid.NewGuid();
        await groupCheckoutFacade.InitiateGroupCheckout(
            new InitiateGroupCheckout(groupCheckoutId, Guid.NewGuid(), guestStayAccountIds, now.AddDays(-1))
        );
        publishedMessages.Reset();

        return groupCheckoutId;
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GroupCheckoutFacade groupCheckoutFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public EntityDefinitionTests()
    {
        groupCheckoutFacade = new GroupCheckoutFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);

        ConfigureGroupCheckouts(messageBus, groupCheckoutFacade);
    }
}
