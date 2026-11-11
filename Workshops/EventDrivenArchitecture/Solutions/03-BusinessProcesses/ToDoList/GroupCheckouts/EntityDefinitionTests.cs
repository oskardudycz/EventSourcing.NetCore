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
        Guid[] guestStayIds = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStayIds, now);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStayIds, now),
                new CheckOutGuest(guestStayIds[0], now, groupCheckoutId),
                new CheckOutGuest(guestStayIds[1], now, groupCheckoutId),
                new CheckOutGuest(guestStayIds[2], now, groupCheckoutId),
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
        await messageBus.Publish([@event], default);

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
        var guestStayId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(guestStayId, Guid.NewGuid(), Guid.NewGuid());
        // And
        var @event = new GuestStayAccountEvent.GuestCheckOutFailed(guestStayId, GuestStayAccountEvent.GuestCheckOutFailed.FailureReason.BalanceNotSettled, now, groupCheckoutId);

        // When
        await messageBus.Publish([@event], default);

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
        var completedGuestStayId = Guid.NewGuid();
        var lastGuestStayId = Guid.NewGuid();
        var groupCheckoutId = await InitiateGroupCheckout(completedGuestStayId, lastGuestStayId);
        await messageBus.Publish([new GuestStayAccountEvent.GuestCheckedOut(completedGuestStayId, now, groupCheckoutId)], default);
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayId, now, groupCheckoutId);

        // When
        await messageBus.Publish([@event], default);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                @event,
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
        await messageBus.Publish([new GuestStayAccountEvent.GuestCheckOutFailed(failedGuestStayId, GuestStayAccountEvent.GuestCheckOutFailed.FailureReason.BalanceNotSettled, now, groupCheckoutId)], default);
        publishedMessages.Reset();
        // And
        var @event = new GuestStayAccountEvent.GuestCheckedOut(lastGuestStayId, now, groupCheckoutId);

        // When
        await messageBus.Publish([@event], default);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                @event,
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
        groupCheckoutFacade = new GroupCheckOutFacade(messageBus);

        messageBus.Use(publishedMessages.Catch);

        ConfigureGroupCheckouts(
            messageBus,
            new GroupCheckOutToDoList(new GroupCheckoutProjection(database), messageBus)
        );
    }
}
