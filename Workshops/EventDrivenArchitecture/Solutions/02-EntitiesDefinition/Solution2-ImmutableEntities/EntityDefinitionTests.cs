using Bogus;
using EntitiesDefinition.Core;
using EntitiesDefinition.Solution2_ImmutableEntities.GroupCheckouts;
using EntitiesDefinition.Solution2_ImmutableEntities.GuestStayAccounts;
using Xunit;
using Xunit.Abstractions;
using Database = EntitiesDefinition.Core.Database;

namespace EntitiesDefinition.Solution2_ImmutableEntities;

using static GuestStayAccountEvent;
using static GuestStayAccountCommand;
using static GroupCheckoutCommand;

public class EntityDefinitionTests
{
    private readonly ITestOutputHelper testOutputHelper;

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
        publishedEvents.ShouldReceiveSingleEvent(new GuestCheckedIn(guestStayId, now));
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
        publishedEvents.ShouldReceiveSingleEvent(new ChargeRecorded(guestStayId, amount, now));
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
        publishedEvents.ShouldReceiveSingleEvent(new PaymentRecorded(guestStayId, amount, now));
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
        publishedEvents.ShouldReceiveSingleEvent(new PaymentRecorded(guestStayId, amount, now));
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
        publishedEvents.ShouldReceiveSingleEvent(new GuestCheckedOut(guestStayId, now));
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
        publishedEvents.ShouldReceiveSingleEvent(new GuestCheckoutFailed(guestStayId, GuestCheckoutFailed.FailureReason.BalanceNotSettled, now));
    }

    [Fact]
    public async Task GroupCheckoutForMultipleGuestStay_ShouldBeInitiated()
    {
        // Given;
        Guid[] guestStays = [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()];

        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[0], now.AddDays(-1)));
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[1], now.AddDays(-1)));
        await guestStayFacade.CheckInGuest(new CheckInGuest(guestStays[2], now.AddDays(-1)));
        publishedEvents.Reset();
        // And
        var groupCheckoutId = Guid.NewGuid();
        var clerkId = Guid.NewGuid();
        var command = new InitiateGroupCheckout(groupCheckoutId, clerkId, guestStays, now);

        // When
        await guestStayFacade.InitiateGroupCheckout(command);

        // Then
        publishedEvents.ShouldReceiveSingleEvent(new GroupCheckoutEvent.GroupCheckoutInitiated(groupCheckoutId, clerkId, guestStays, now));
    }

    private readonly Database database = new();
    private readonly EventBus eventBus = new();
    private readonly EventCatcher publishedEvents = new();
    private readonly GuestStayFacade guestStayFacade;
    private readonly Faker generate = new();
    private readonly DateTimeOffset now = DateTimeOffset.Now;

    public EntityDefinitionTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        guestStayFacade = new GuestStayFacade(database, eventBus);
        eventBus.Use(publishedEvents.Catch);
    }
}
