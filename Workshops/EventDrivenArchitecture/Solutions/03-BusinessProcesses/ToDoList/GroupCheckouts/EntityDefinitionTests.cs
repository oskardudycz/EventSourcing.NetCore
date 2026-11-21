using BusinessProcesses.Core;
using BusinessProcesses.ToDoList.GuestStayAccounts;
using Xunit;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.GroupCheckouts;

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
        await messageBus.Publish([@event], TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                @event,
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
        await messageBus.Publish([@event], TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                @event,
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
        await messageBus.Publish([new GuestStayAccountEvent.GuestCheckedOut(completedGuestStayAccountId, now, groupCheckoutId)], TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayAccountId, now, groupCheckoutId);

        // When
        await messageBus.Publish([@event], TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                @event,
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
        await messageBus.Publish([new GuestStayAccountEvent.GuestCheckoutFailed(failedGuestStayAccountId, GuestStayAccountEvent.GuestCheckoutFailed.FailureReason.BalanceNotSettled, now, groupCheckoutId)], TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayAccountId, now, groupCheckoutId);

        // When
        await messageBus.Publish([@event], TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                @event,
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
        groupCheckoutFacade = new GroupCheckoutFacade(messageBus);

        messageBus.Use(publishedMessages.Catch);

        ConfigureGroupCheckouts(
            messageBus,
            new GroupCheckoutToDoList(new GroupCheckoutProjection(database), messageBus)
        );
    }
}
