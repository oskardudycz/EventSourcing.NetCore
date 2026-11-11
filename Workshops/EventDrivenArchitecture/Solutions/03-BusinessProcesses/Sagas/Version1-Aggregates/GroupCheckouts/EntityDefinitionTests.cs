using BusinessProcesses.Core;
using Xunit;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Sagas.Version1_Aggregates.GroupCheckouts;

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
        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, guestStayId, now);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

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
        var command = new RecordGuestCheckoutFailure(groupCheckoutId, guestStayId, now);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutFailure(command);

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
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(new RecordGuestCheckoutCompletion(groupCheckoutId, completedGuestStayId, now));
        publishedMessages.Reset();
        // And
        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, lastGuestStayId, now);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

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
        await groupCheckoutFacade.RecordGuestCheckoutFailure(new RecordGuestCheckoutFailure(groupCheckoutId, failedGuestStayId, now));
        publishedMessages.Reset();
        // And
        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, lastGuestStayId, now);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

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
