using Bogus;
using Consistency.Core;
using Consistency.Version2_ImmutableEntities.GuestStayAccounts;
using Xunit;
using Database = Consistency.Core.Database;

namespace Consistency.Version2_ImmutableEntities;

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
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.Reset();

        // When
        await guestStayFacade.CheckInGuest(command, TestContext.Current.CancellationToken);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedIn(guestStayAccountId, now));
    }

    [Fact]
    public async Task RecordingChargeForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordCharge(guestStayAccountId, amount, now);

        // When
        await guestStayFacade.RecordCharge(command, TestContext.Current.CancellationToken);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new ChargeRecorded(guestStayAccountId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordPayment(guestStayAccountId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayAccountId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuestWithCharge_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, generate.Finance.Amount(), now.AddHours(-1)), TestContext.Current.CancellationToken);
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordPayment(guestStayAccountId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command, TestContext.Current.CancellationToken);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayAccountId, amount, now));
    }

    [Fact]
    public async Task CheckingOutGuestWithSettledBalance_Succeeds()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();

        var amount = generate.Finance.Amount();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, amount, now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayAccountId, amount, now.AddHours(-1)), TestContext.Current.CancellationToken);
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.Reset();
        // And
        var command = new CheckOutGuest(guestStayAccountId, now);

        // When
        await guestStayFacade.CheckOutGuest(command, TestContext.Current.CancellationToken);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayAccountId, now));
    }

    [Fact]
    public async Task CheckingOutGuestWithUnsettledBalance_FailsWithGuestCheckoutFailed()
    {
        // Given
        var guestStayAccountId = Guid.NewGuid();

        var amount = generate.Finance.Amount();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayAccountId, now.AddDays(-1)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayAccountId, amount + 10, now.AddHours(-2)), TestContext.Current.CancellationToken);
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayAccountId, amount, now.AddHours(-1)), TestContext.Current.CancellationToken);
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
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
        await backgroundJobs.RunUntilIdle(publishedMessages, TestContext.Current.CancellationToken);
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckoutFailed(guestStayAccountId, GuestCheckoutFailed.FailureReason.BalanceNotSettled, now));
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly BackgroundJobs backgroundJobs = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayAccountFacade guestStayFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public EntityDefinitionTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayAccountFacade(database, messageBus, backgroundJobs);
        messageBus.Use(publishedMessages.Catch);
    }
}
