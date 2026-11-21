using OptimisticConcurrency.Core;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class IdempotencyTests
{
    [Fact]
    public async Task GuestIsCheckedInOnceForTheSameGuestStayAccount()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        var command = new CheckInGuest(guestStayId, now);
        // And
        await guestStayFacade.CheckInGuest(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.CheckInGuest(command, TestContext.Current.CancellationToken);

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
        await guestStayFacade.RecordCharge(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.RecordCharge(command, TestContext.Current.CancellationToken);

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
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

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
        await guestStayFacade.CheckOutGuest(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.CheckOutGuest(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayId, now, groupCheckoutId));
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
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public IdempotencyTests()
    {
        guestStayFacade = new GuestStayFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);
    }
}
