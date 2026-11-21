using Bogus;
using Idempotency.Core;
using Xunit;
using Database = Idempotency.Core.Database;

namespace Idempotency.Version1_Aggregates.GuestStayAccounts;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;

public class EntityDefinitionTests
{
    [Fact]
    public async Task CheckingInGuest_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        var command = new CheckInGuest(guestStayAccountId, now);
        publishedMessages.Reset();

        // When
        await guestStayFacade.CheckInGuest(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedIn(guestStayAccountId, now));
    }

    [Fact]
    public async Task RecordingChargeForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var chargeId = Guid.NewGuid();
        var command = new RecordCharge(guestStayAccountId, chargeId, amount, now);

        // When
        await guestStayFacade.RecordCharge(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new ChargeRecorded(guestStayAccountId, chargeId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var paymentId = Guid.NewGuid();
        var command = new RecordPayment(guestStayAccountId, paymentId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayAccountId, paymentId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuestWithCharge_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, Guid.NewGuid(), generate.Finance.Amount(), now.AddHours(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var paymentId = Guid.NewGuid();
        var command = new RecordPayment(guestStayAccountId, paymentId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayAccountId, paymentId, amount, now));
    }

    [Fact]
    public async Task CheckingOutGuestWithSettledBalance_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();

        var amount = generate.Finance.Amount();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, Guid.NewGuid(), amount, now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayAccountId, Guid.NewGuid(), amount, now.AddHours(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var command = new CheckOutGuest(guestStayAccountId, now);

        // When
        await guestStayFacade.CheckOutGuest(command, TestContext.Current.CancellationToken);

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayAccountId, now));
    }

    [Fact]
    public async Task CheckingOutGuestWithUnsettledBalance_FailsWithGuestCheckoutFailed()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();

        var amount = generate.Finance.Amount();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, Guid.NewGuid(), amount + 10, now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayAccountId, Guid.NewGuid(), amount, now.AddHours(-1)), TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var command = new CheckOutGuest(guestStayAccountId, now);

        // When
        try
        {
            await guestStayFacade.CheckOutGuest(command, TestContext.Current.CancellationToken);
        }
        catch (Exception exc)
        {
            testOutputHelper.WriteLine(exc.Message);
        }

        // Then
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckoutFailed(guestStayAccountId, GuestCheckoutFailed.FailureReason.BalanceNotSettled, now));
    }

    [Fact]
    public async Task RecordingChargeForMissingGuestStayAccount_FailsWithEntityNotFound()
    {
        // Given
        var command = new RecordCharge(Guid.NewGuid(), Guid.NewGuid(), generate.Finance.Amount(), now);
        publishedMessages.Reset();

        // When
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await guestStayFacade.RecordCharge(command, TestContext.Current.CancellationToken));

        // Then
        Assert.Equal("Entity not found", exception.Message);
        publishedMessages.ShouldNotReceiveAnyMessage();
    }

    [Fact]
    public async Task RecordingPaymentForMissingGuestStayAccount_FailsWithEntityNotFound()
    {
        // Given
        var command = new RecordPayment(Guid.NewGuid(), Guid.NewGuid(), generate.Finance.Amount(), now);
        publishedMessages.Reset();

        // When
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken));

        // Then
        Assert.Equal("Entity not found", exception.Message);
        publishedMessages.ShouldNotReceiveAnyMessage();
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayAccountFacade guestStayFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public EntityDefinitionTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayAccountFacade(database, messageBus);
        messageBus.Use(publishedMessages.Catch);
    }
}
