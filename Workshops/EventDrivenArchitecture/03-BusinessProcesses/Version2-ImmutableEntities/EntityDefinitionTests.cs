using Bogus;
using BusinessProcesses.Core;
using BusinessProcesses.Version2_ImmutableEntities.GroupCheckouts;
using BusinessProcesses.Version2_ImmutableEntities.GuestStayAccounts;
using Xunit;
using Xunit.Abstractions;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.Version2_ImmutableEntities;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class EntityDefinitionTests
{
    [Fact]
    public async Task CheckingInGuest_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        var command = new CheckInGuest(guestStayId, now);
        publishedEvents.Reset();

        // When
        await guestStayFacade.CheckInGuest(command);

        // Then
        publishedEvents.ShouldReceiveSingleMessage(new GuestCheckedIn(guestStayId, now));
    }

    [Fact]
    public async Task RecordingChargeForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        publishedEvents.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordCharge(guestStayId, amount, now);

        // When
        await guestStayFacade.RecordCharge(command);

        // Then
        publishedEvents.ShouldReceiveSingleMessage(new ChargeRecorded(guestStayId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuest_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        publishedEvents.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordPayment(guestStayId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command);

        // Then
        publishedEvents.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayId, amount, now));
    }

    [Fact]
    public async Task RecordingPaymentForCheckedInGuestWithCharge_Succeeds()
    {
        // Given
        var guestStayId = Guid.NewGuid();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayId, generate.Finance.Amount(), now.AddHours(-1)));
        publishedEvents.Reset();
        // And
        var amount = generate.Finance.Amount();
        var command = new RecordPayment(guestStayId, amount, now);

        // When
        await guestStayFacade.RecordPayment(command);

        // Then
        publishedEvents.ShouldReceiveSingleMessage(new PaymentRecorded(guestStayId, amount, now));
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
        publishedEvents.Reset();
        // And
        var command = new CheckOutGuest(guestStayId, now);

        // When
        await guestStayFacade.CheckOutGuest(command);

        // Then
        publishedEvents.ShouldReceiveSingleMessage(new GuestCheckedOut(guestStayId, now));
    }

    [Fact]
    public async Task CheckingOutGuestWithSettledBalance_FailsWithGuestCheckoutFailed()
    {
        // Given
        var guestStayId = Guid.NewGuid();

        var amount = generate.Finance.Amount();
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStayId, now.AddDays(-1)));
        await guestStayFacade.RecordCharge(new RecordCharge(guestStayId, amount + 10, now.AddHours(-2)));
        await guestStayFacade.RecordPayment(new RecordPayment(guestStayId, amount, now.AddHours(-1)));
        publishedEvents.Reset();
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
        publishedEvents.ShouldReceiveSingleMessage(new GuestCheckoutFailed(guestStayId, GuestCheckoutFailed.FailureReason.BalanceNotSettled, now));
    }

    private readonly Database database = new();
    private readonly EventBus eventBus = new();
    private readonly EventCatcher publishedEvents = new();
    private readonly GuestStayFacade guestStayFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;
    private readonly ITestOutputHelper testOutputHelper;

    public EntityDefinitionTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayFacade(database, eventBus);
        eventBus.Use(publishedEvents.Catch);
    }
}
