using OptimisticConcurrency.Core;
using Xunit;
using Database = OptimisticConcurrency.Core.Database;

namespace OptimisticConcurrency.RetryInInfrastructure.Version2_ImmutableEntities.GuestStayAccounts;

using static GuestStayAccountCommand;
using static GuestStayAccountEvent;

public class IdempotencyTests
{
    [Fact]
    public async Task GuestIsCheckedInOnceForTheSameGuestStayAccount()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var command = new CheckInGuest(guestStayAccountId, now);
        // And
        await guestStayFacade.CheckInGuest(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.CheckInGuest(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedIn(guestStayAccountId, now));
    }

    [Fact]
    public async Task ChargeIsRecordedOnceForTheSameChargeId()
    {
        // Given
        var guestStayAccountId = await CheckInGuest();
        var chargeId = Guid.NewGuid();
        var command = new RecordCharge(guestStayAccountId, chargeId, 100, now);
        // And
        await guestStayFacade.RecordCharge(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.RecordCharge(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new ChargeRecorded(guestStayAccountId, chargeId, 100, now));
    }

    [Fact]
    public async Task PaymentIsRecordedOnceForTheSamePaymentId()
    {
        // Given
        var guestStayAccountId = await CheckInGuest();
        var paymentId = Guid.NewGuid();
        var command = new RecordPayment(guestStayAccountId, paymentId, 100, now);
        // And
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayAccountId, paymentId, 100, now));
    }

    [Fact]
    public async Task GuestIsCheckedOutOnceForTheSameGroupCheckout()
    {
        // Given
        var guestStayAccountId = await CheckInGuest();
        var groupCheckoutId = Guid.NewGuid();
        var command = new CheckOutGuest(guestStayAccountId, now, groupCheckoutId);
        // And
        await guestStayFacade.CheckOutGuest(command, TestContext.Current.CancellationToken);

        // When
        await guestStayFacade.CheckOutGuest(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayAccountId, now, groupCheckoutId));
    }

    private async Task<Guid> CheckInGuest()
    {
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now));
        publishedMessages.Reset();
        return guestStayAccountId;
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayAccountFacade guestStayFacade;
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public IdempotencyTests()
    {
        guestStayFacade = new GuestStayAccountFacade(database, messageBus);

        messageBus.Use(publishedMessages.Catch);
    }
}
