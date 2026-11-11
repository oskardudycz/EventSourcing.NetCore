using Bogus;
using Idempotency.Core;
using Idempotency.Version1_Aggregates.GroupCheckouts;
using Idempotency.Version1_Aggregates.GuestStayAccounts;
using Xunit;
using Xunit.Abstractions;
using Database = Idempotency.Core.Database;
using GroupCheckoutCommand = Idempotency.Version1_Aggregates.GroupCheckouts.GroupCheckoutCommand;

namespace Idempotency.Version1_Aggregates;

using static GroupCheckoutCommand;
using static GroupCheckoutEvent;
using static GuestStayAccountCommand;
using static GuestStayAccountEvent;
using static GroupCheckoutsConfig;
using static GuestStayAccountsConfig;

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
        await groupCheckoutFacade.InitiateGroupCheckout(command);

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(command);

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
            now.AddDays(-1)));
        publishedMessages.Reset();


        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now);
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

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
            now.AddDays(-1)));
        publishedMessages.Reset();


        var command = new RecordGuestCheckoutFailure(groupCheckoutId, guestStays[0], now);
        await groupCheckoutFacade.RecordGuestCheckoutFailure(command);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutFailure(command);

        // Then

        publishedMessages.ShouldReceiveSingleMessage(
            new GroupCheckoutEvent.GuestCheckOutFailed(groupCheckoutId, guestStays[0], now));
    }

    [Fact]
    public async Task GuestIsCheckedInOnceForTheSameGuestStayAccount()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        var command = new CheckInGuest(guestStayId, now);
        // And
        await guestStayFacade.CheckInGuest(command);

        // When
        await guestStayFacade.CheckInGuest(command);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedIn(guestStayId, now));
    }

    [Fact]
    public async Task ChargeIsRecordedOnceForTheSameChargeId()
    {
        // Given
        var guestStayId = await CheckInGuest();
        var chargeId = Guid.NewGuid();
        var command = new RecordCharge(guestStayId, chargeId, 100, now);
        // And
        await guestStayFacade.RecordCharge(command);

        // When
        await guestStayFacade.RecordCharge(command);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new ChargeRecorded(guestStayId, chargeId, 100, now));
    }

    [Fact]
    public async Task PaymentIsRecordedOnceForTheSamePaymentId()
    {
        // Given
        var guestStayId = await CheckInGuest();
        var paymentId = Guid.NewGuid();
        var command = new RecordPayment(guestStayId, paymentId, 100, now);
        // And
        await guestStayFacade.RecordPayment(command);

        // When
        await guestStayFacade.RecordPayment(command);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayId, paymentId, 100, now));
    }

    [Fact]
    public async Task GuestIsCheckedOutOnceForTheSameGroupCheckout()
    {
        // Given
        var guestStayId = await CheckInGuest();
        var groupCheckoutId = Guid.NewGuid();
        var command = new CheckOutGuest(guestStayId, now, groupCheckoutId);
        // And
        await guestStayFacade.CheckOutGuest(command);

        // When
        await guestStayFacade.CheckOutGuest(command);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayId, now, groupCheckoutId));
    }

    [Fact]
    public async Task GroupCheckoutCompletesOnceWhenTheLastGuestCheckoutCompletionIsRecordedTwice()
    {
        // Given
        var groupCheckoutId = Guid.NewGuid();
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid()];
        await groupCheckoutFacade.InitiateGroupCheckout(
            new InitiateGroupCheckout(groupCheckoutId, Guid.NewGuid(), guestStays, now));
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(
            new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[0], now));
        publishedMessages.Reset();
        // And
        var command = new RecordGuestCheckoutCompletion(groupCheckoutId, guestStays[1], now);
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

        // When
        await groupCheckoutFacade.RecordGuestCheckoutCompletion(command);

        // Then
        publishedMessages.ShouldReceiveMessages(
            [
                new GuestCheckoutCompleted(groupCheckoutId, guestStays[1], now),
                new GroupCheckoutCompleted(groupCheckoutId, guestStays, now)
            ]
        );
    }

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
            await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now));
        publishedMessages.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();

        // When
        await groupCheckoutFacade.InitiateGroupCheckout(
            new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now));

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

    private async Task<Guid> CheckInGuest()
    {
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now));
        publishedMessages.Reset();
        return guestStayId;
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayFacade guestStayFacade;
    private readonly GroupCheckOutFacade groupCheckoutFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public IdempotencyTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayFacade(database, messageBus);
        groupCheckoutFacade = new GroupCheckOutFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);
    }
}
