using OptimisticConcurrency.Core;
using OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GroupCheckouts;
using OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GuestStayAccounts;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;
using GroupCheckoutCommand = OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates.GroupCheckouts.GroupCheckoutCommand;

namespace OptimisticConcurrency.RetryInInfrastructure.Version1_Aggregates;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;
using static GuestStayAccountEvent;
using static GroupCheckoutsConfig;
using static GuestStayAccountsConfig;

public class IdempotencyTests
{
    [Fact]
    public async Task GroupCheckoutCompletesOnceWhenTheBusDeliversEveryMessageTwice()
    {
        // Given
        var database = new Database();
        var messageBus = new MessageBus(deliverTwice: true);
        var publishedMessages = new MessageCatcher();
        messageBus.Use(publishedMessages.Catch);

        var guestStayFacade = new GuestStayFacade(database, messageBus);
        var groupCheckoutFacade = new GroupCheckOutFacade(database, messageBus);
        ConfigureGuestStayAccounts(messageBus, guestStayFacade);
        ConfigureGroupCheckouts(messageBus, groupCheckoutFacade);

        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid()];
        foreach (var guestStayId in guestStays)
            await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now), TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[0], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStays[0], now),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId),
                new GuestCheckedOut(guestStays[1], now, groupCheckoutId),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[1], now),
                new GuestCheckoutCompleted(groupCheckoutId, guestStays[1], now),
                new GroupCheckoutCompleted(groupCheckoutId, guestStays, now),
                new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[1], now),
                new CheckOutGuest(guestStays[0], now, groupCheckoutId),
                new CheckOutGuest(guestStays[1], now, groupCheckoutId)
            ]
        );
    }

    private readonly DateTimeOffset now = DateTimeOffset.Now;
}
