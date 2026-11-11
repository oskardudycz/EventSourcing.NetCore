using BusinessProcesses.Core;
using BusinessProcesses.Choreography.GuestStayAccounts;
using Xunit;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Choreography.GroupCheckouts;

using static GroupCheckoutCommand;

public class EntityDefinitionTests
{
    [Fact]
    public async Task InitiatingGroupCheckout_Succeeds()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        Guid[] guestStayIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStayIds, now);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayIds, now),
            ]
        );
    }

    [Fact]
    public async Task RecordingGuestCheckoutCompletion_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(guestStayId, Guid.NewGuid(), Guid.NewGuid());
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(guestStayId, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.GuestCheckedOut(@event);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, guestStayId, now),
            ]
        );
    }

    [Fact]
    public async Task RecordingGuestCheckoutFailure_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(guestStayId, Guid.NewGuid(), Guid.NewGuid());
        // And
        var @event = new GuestStayAccountEvent.GuestCheckOutFailed(guestStayId, GuestStayAccountEvent.GuestCheckOutFailed.FailureReason.BalanceNotSettled, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.GuestCheckOutFailed(@event);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStayId, now),
            ]
        );
    }

    [Fact]
    public async Task RecordingLastGuestCheckoutCompletion_CompletesGroupCheckout()
    {
        // Given
        var completedGuestStayId = Guid.NewGuid();
        var lastGuestStayId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(completedGuestStayId, lastGuestStayId);
        await groupCheckoutFacade.GuestCheckedOut(new GuestStayAccountEvent.GuestCheckedOut(completedGuestStayId, now, groupCheckoutId));
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayId, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.GuestCheckedOut(@event);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, lastGuestStayId, now),
                new GroupCheckoutEvent.GroupCheckoutCompleted(groupCheckoutId, [completedGuestStayId, lastGuestStayId], now),
            ]
        );
    }

    [Fact]
    public async Task RecordingLastGuestCheckoutAfterFailure_FailsGroupCheckout()
    {
        // Given
        var failedGuestStayId = Guid.NewGuid();
        var lastGuestStayId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(failedGuestStayId, lastGuestStayId);
        await groupCheckoutFacade.GuestCheckOutFailed(new GuestStayAccountEvent.GuestCheckOutFailed(failedGuestStayId, GuestStayAccountEvent.GuestCheckOutFailed.FailureReason.BalanceNotSettled, now, groupCheckoutId));
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayId, now, groupCheckoutId);

        // When
        await groupCheckoutFacade.GuestCheckedOut(@event);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GuestCheckoutCompleted(groupCheckoutId, lastGuestStayId, now),
                new GroupCheckoutEvent.GroupCheckoutFailed(groupCheckoutId, [lastGuestStayId], [failedGuestStayId], now),
            ]
        );
    }

    private async Task<Guid> InitiateGroupCheckout(params Guid[] guestStayIds)
    {
        var groupCheckoutId = Guid.NewGuid();
        await groupCheckoutFacade.InitiateGroupCheckout(
            new InitiateGroupCheckout(groupCheckoutId, Guid.NewGuid(), guestStayIds, now.AddDays(-1))
        );
        publishedMessages.Reset();

        return groupCheckoutId;
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GroupCheckOutFacade groupCheckoutFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public EntityDefinitionTests()
    {
        groupCheckoutFacade = new GroupCheckOutFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);
    }
}
