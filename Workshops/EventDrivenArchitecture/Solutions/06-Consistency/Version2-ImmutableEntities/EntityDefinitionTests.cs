using Bogus;
using Consistency.Core;
using Consistency.Version2_ImmutableEntities.GuestStayAccounts;
using Xunit;
using Xunit.Abstractions;
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
        var guestStayId = Guid.NewGuid();
        var command = new CheckInGuest(guestStayId, now);
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();

        // When
        await guestStayFacade.CheckInGuest(command);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedIn(guestStayId, now));
    }

    [Fact]
    public async Task RecordingChargeForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordCharge(guestStayId, amount, now);

        // When
        await guestStayFacade.RecordCharge(command);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.ShouldReceiveSingleMessage(new ChargeRecorded(guestStayId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordPayment(guestStayId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuestWithCharge_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayId, generate.Finance.Amount(), now.AddHours(-1)));
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordPayment(guestStayId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayId, amount, now));
    }

    [Fact]
    public async Task CheckingOutGuestWithSettledBalance_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();

        var amount = generate.Finance.Amount();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayId, amount, now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayId, amount, now.AddHours(-1)));
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();
        // And
        var command = new CheckOutGuest(guestStayId, now);

        // When
        await guestStayFacade.CheckOutGuest(command);

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayId, now));
    }

    [Fact]
    public async Task CheckingOutGuestWithUnsettledBalance_FailsWithGuestCheckoutFailed()
    {
        // Given
        var guestStayId = Guid.NewGuid();

        var amount = generate.Finance.Amount();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayId, amount + 10, now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayId, amount, now.AddHours(-1)));
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.Reset();
        // And
        var command = new CheckOutGuest(guestStayId, now);

        // When
        try
        {
            await guestStayFacade.CheckOutGuest(command);
        }
        catch (Exception exc)
        {
            testOutputHelper.WriteLine(exc.Message);
        }

        // Then
        await backgroundJobs.RunUntilIdle(publishedMessages);
        publishedMessages.ShouldReceiveSingleMessage(new GuestCheckOutFailed(guestStayId, GuestCheckOutFailed.FailureReason.BalanceNotSettled, now));
    }

    private readonly Database database = new();
    private readonly MessageBus messageBus = new();
    private readonly BackgroundJobs backgroundJobs = new();
    private readonly MessageCatcher publishedMessages = new();
    private readonly GuestStayFacade guestStayFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public EntityDefinitionTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayFacade(database, messageBus, backgroundJobs);
        messageBus.Use(publishedMessages.Catch);
    }
}
